using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartFarmSEP490.Model;
using SmartFarmSEP490.Model.DTOs;
using SmartFarmSEP490.Model.Enums;
using SmartFarmSEP490.Repository.Interfaces.AI;
using SmartFarmSEP490.Service.Interfaces.AI;
using SmartFarmSEP490.Service.Interfaces.Commons;
using SmartFarmSEP490.Service.Services.AI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace SmartFarmSEP490.Service.Services.AI;

/// <summary>
/// Service chính xử lý AI: enqueue, gọi AI, parse, persist.
/// Inject từ API controller (để enqueue) và từ BackgroundService (để process).
/// </summary>
public class AIAnalysisService : IAIAnalysisService
{
    private readonly IAIAnalysisRepository _aiRepo;
    private readonly SmartFarmSEP490.Repository.DbContexts.SmartFarmDbContext _db;
    private readonly IHttpClientFactory _httpFactory;
    private readonly ICloudinaryService _cloudinary;
    private readonly AIOptions _options;
    private readonly ILogger<AIAnalysisService> _logger;
    private readonly Channel<Guid> _queue;

    public AIAnalysisService(
        IAIAnalysisRepository aiRepo,
        SmartFarmSEP490.Repository.DbContexts.SmartFarmDbContext db,
        IHttpClientFactory httpFactory,
        ICloudinaryService cloudinary,
        IOptions<AIOptions> options,
        ILogger<AIAnalysisService> logger,
        Channel<Guid> queue)
    {
        _aiRepo = aiRepo;
        _db = db;
        _httpFactory = httpFactory;
        _cloudinary = cloudinary;
        _options = options.Value;
        _logger = logger;
        _queue = queue;
    }

    // ------------------------------------------------------------------
    // 1) ENQUEUE
    // ------------------------------------------------------------------

    public async Task EnqueueAsync(Guid plantImageId, CancellationToken ct = default)
    {
        var image = await _db.PlantImages.FindAsync(new object[] { plantImageId }, ct);
        if (image == null) throw new InvalidOperationException($"PlantImage {plantImageId} not found.");

        // Idempotent: nếu đã có AIAnalysis Completed thì bỏ qua
        var existing = await _aiRepo.GetByPlantImageIdAsync(plantImageId);
        if (existing?.AIStatus == AIStatus.Completed)
        {
            _logger.LogInformation("[AI] PlantImage {Id} already has Completed AIAnalysis — skip enqueue.", plantImageId);
            return;
        }

        await _queue.Writer.WriteAsync(plantImageId, ct);
        _logger.LogInformation("[AI] Enqueued PlantImage {Id}.", plantImageId);
    }

    // ------------------------------------------------------------------
    // 2) PROCESS (Worker gọi)
    // ------------------------------------------------------------------

    public async Task<AIAnalysis> ProcessAsync(Guid plantImageId, CancellationToken ct = default)
    {
        var image = await _db.PlantImages.FindAsync(new object[] { plantImageId }, ct);
        if (image == null) throw new InvalidOperationException($"PlantImage {plantImageId} not found.");

        var provider = ChooseProvider(image);

        // Lấy / tạo AIAnalysis
        var analysis = await _aiRepo.GetByPlantImageIdAsync(plantImageId);
        var isNew = false;

        // FK an toàn: chỉ set TaskReportId khi PlantImage có thật (NULL khi không có TaskReport)
        Guid? taskReportId = image.TaskReportId;
        if (analysis == null)
        {
            analysis = new AIAnalysis
            {
                Id           = Guid.NewGuid(),
                PlantImageId = image.Id,
                TaskReportId = taskReportId,
                AIProvider   = provider,
                AIStatus     = AIStatus.Processing,
                RequestedAt  = DateTime.UtcNow
            };
            isNew = true;
        }
        else
        {
            analysis.AIProvider   = provider;
            analysis.AIStatus     = AIStatus.Processing;
            analysis.RequestedAt  = DateTime.UtcNow;
            analysis.ErrorMessage = null;
            // KHÔNG cập nhật TaskReportId khi retry — giữ giá trị cũ (đã được FK validate lúc insert)
        }

        image.AIStatus = AIStatus.Processing;

        if (isNew) await _aiRepo.AddAsync(analysis);
        else _aiRepo.Update(analysis);
        await _aiRepo.SaveChangesAsync();

        try
        {
            var parsed = await CallAiWithRetryAsync(image, provider, ct);

            // ----- Merge kết quả -----
            analysis.AIProvider        = parsed.AIProvider;
            analysis.ApiVersion        = parsed.ApiVersion;
            analysis.FinalStatus       = parsed.FinalStatus;
            analysis.IsHealthy         = parsed.IsHealthy;
            analysis.Label             = parsed.Label;
            analysis.Confidence        = parsed.Confidence;
            analysis.GateLabel         = parsed.GateLabel;
            analysis.GateConfidence    = parsed.GateConfidence;
            analysis.DetectionCount    = parsed.DetectionCount;
            analysis.BestBoxX1         = parsed.BestBoxX1;
            analysis.BestBoxY1         = parsed.BestBoxY1;
            analysis.BestBoxX2         = parsed.BestBoxX2;
            analysis.BestBoxY2         = parsed.BestBoxY2;
            analysis.RawResultJson     = parsed.RawResultJson;
            analysis.ProbabilitiesJson = parsed.ProbabilitiesJson;
            analysis.AnnotatedImageUrl = parsed.AnnotatedImageUrl;
            analysis.ErrorMessage      = null;
            analysis.AIStatus          = AIStatus.Completed;
            analysis.CompletedAt       = DateTime.UtcNow;

            image.AIStatus            = AIStatus.Completed;
            image.AIPredictedLabel    = parsed.Label;
            image.AIConfidence        = parsed.Confidence;
            image.AIConfidenceRate    = parsed.Confidence.HasValue
                ? Math.Round(parsed.Confidence.Value * 100, 2)
                : null;
            if (!string.IsNullOrEmpty(parsed.AnnotatedImageUrl))
                image.AIAnnotatedImageUrl = parsed.AnnotatedImageUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[AI] Process failed for PlantImage {Id}", plantImageId);
            analysis.AIStatus     = AIStatus.Failed;
            analysis.ErrorMessage = ex.Message;
            analysis.CompletedAt  = DateTime.UtcNow;

            image.AIStatus = AIStatus.Failed;
        }

        _aiRepo.Update(analysis);
        await _aiRepo.SaveChangesAsync();
        return analysis;
    }

    // ------------------------------------------------------------------
    // 3) HTTP call + retry + parse
    // ------------------------------------------------------------------

    private async Task<AIAnalysis> CallAiWithRetryAsync(
        PlantImage image, AIProvider provider, CancellationToken ct)
    {
        var imageBytes = await DownloadImageAsync(image.ImageUrl, ct);

        Exception? lastEx = null;
        int[] backoffs = { 2, 5, 15 }; // giây

        for (int attempt = 0; attempt <= _options.MaxRetries; attempt++)
        {
            try
            {
                using var content = new MultipartFormDataContent();
                content.Add(new ByteArrayContent(imageBytes), "file", $"upload_{image.Id}.jpg");

                var (url, extraForm) = BuildRequest(provider);
                foreach (var kv in extraForm)
                    content.Add(new StringContent(kv.Value), kv.Key);

                using var http = _httpFactory.CreateClient("ai-backend");
                using var resp = await http.PostAsync(url, content, ct);

                if (!resp.IsSuccessStatusCode)
                {
                    var body = await resp.Content.ReadAsStringAsync(ct);
                    if ((int)resp.StatusCode >= 400 && (int)resp.StatusCode < 500 && (int)resp.StatusCode != 408)
                        throw new InvalidOperationException($"AI {(int)resp.StatusCode}: {body}");
                    throw new HttpRequestException($"AI {(int)resp.StatusCode}: {body}");
                }

                var json = await resp.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: ct);

                return provider switch
                {
                    AIProvider.TomatoLeafDiseaseOnnx => TomatoMapper.Parse(image, json),
                    AIProvider.ArgoPestOnnx          => (await ArgoPestMapper.ParseAsync(image, json, _cloudinary, ct)).analysis,
                    _ => throw new InvalidOperationException($"Unknown provider {provider}")
                };
            }
            catch (HttpRequestException ex) when (attempt < _options.MaxRetries)
            {
                lastEx = ex;
                var delay = TimeSpan.FromSeconds(backoffs[Math.Min(attempt, backoffs.Length - 1)]);
                _logger.LogWarning(ex,
                    "[AI] HTTP attempt {Attempt}/{Max} failed for PlantImage {Id}. Retrying in {Delay}s.",
                    attempt + 1, _options.MaxRetries, image.Id, delay.TotalSeconds);
                await Task.Delay(delay, ct);
            }
            catch (TaskCanceledException ex) when (attempt < _options.MaxRetries)
            {
                lastEx = ex;
                var delay = TimeSpan.FromSeconds(backoffs[Math.Min(attempt, backoffs.Length - 1)]);
                _logger.LogWarning(ex,
                    "[AI] Timeout attempt {Attempt}/{Max} for PlantImage {Id}. Retrying in {Delay}s.",
                    attempt + 1, _options.MaxRetries, image.Id, delay.TotalSeconds);
                await Task.Delay(delay, ct);
            }
            catch (JsonException ex) when (attempt < _options.MaxRetries)
            {
                // Parse fail — không retry thường, nhưng cũng không nên loop vô tận
                lastEx = ex;
                var delay = TimeSpan.FromSeconds(backoffs[Math.Min(attempt, backoffs.Length - 1)]);
                _logger.LogWarning(ex,
                    "[AI] JSON parse attempt {Attempt}/{Max} failed. Retrying in {Delay}s.",
                    attempt + 1, _options.MaxRetries, delay.TotalSeconds);
                await Task.Delay(delay, ct);
            }
        }

        throw lastEx ?? new InvalidOperationException("AI call failed after retries.");
    }

    private (string url, Dictionary<string, string> form) BuildRequest(AIProvider provider) => provider switch
    {
        AIProvider.TomatoLeafDiseaseOnnx => (
            $"{_options.TomatoUrl.TrimEnd('/')}/predict",
            new Dictionary<string, string>
            {
                ["gate_threshold"] = "0.60",
                ["detect_conf"]    = "0.25",
                ["detect_iou"]     = "0.45",
                ["max_detections"] = "3",
                ["include_probabilities"] = "true"
            }),
        AIProvider.ArgoPestOnnx => (
            $"{_options.ArgoPestUrl.TrimEnd('/')}/predict",
            new Dictionary<string, string>
            {
                ["conf"] = "0.40",
                ["iou"]  = "0.45"
            }),
        _ => throw new InvalidOperationException($"Unknown provider {provider}")
    };

    private async Task<byte[]> DownloadImageAsync(string url, CancellationToken ct)
    {
        using var http = _httpFactory.CreateClient("image-downloader");
        return await http.GetByteArrayAsync(url, ct);
    }

    // ------------------------------------------------------------------
    // 4) FE poll
    // ------------------------------------------------------------------

    public async Task<List<PlantImageWithAnalysisDto>> GetByTaskReportAsync(
        Guid taskReportId, bool includeAnalysis, CancellationToken ct = default)
    {
        var images = await _db.PlantImages
            .AsNoTracking()
            .Where(p => p.TaskReportId == taskReportId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(ct);

        var result = new List<PlantImageWithAnalysisDto>(images.Count);
        foreach (var img in images)
        {
            var dto = new PlantImageWithAnalysisDto
            {
                Id                 = img.Id,
                TaskReportId       = img.TaskReportId,
                BatchId            = img.BatchId,
                ExperimentId       = img.ExperimentId,
                ImageUrl           = img.ImageUrl,
                Caption            = img.Caption,
                CapturedAt         = img.CapturedAt,
                CreatedAt          = img.CreatedAt,
                AIStatus           = img.AIStatus.ToString(),
                AIProvider         = img.AIProvider?.ToString(),
                AIPredictedLabel   = img.AIPredictedLabel,
                AIConfidence       = img.AIConfidence,
                AIConfidenceRate   = img.AIConfidenceRate,
                AIAnnotatedImageUrl = img.AIAnnotatedImageUrl
            };

            if (includeAnalysis)
            {
                var analysis = await _aiRepo.GetByPlantImageIdAsync(img.Id);
                if (analysis != null)
                {
                    dto.AIAnalysis = MapToDto(analysis);
                }
            }

            result.Add(dto);
        }
        return result;
    }

    private static AIAnalysisDto MapToDto(AIAnalysis a) => new()
    {
        Id                = a.Id,
        PlantImageId      = a.PlantImageId,
        TaskReportId      = a.TaskReportId,
        AIProvider        = a.AIProvider.ToString(),
        ApiVersion        = a.ApiVersion,
        FinalStatus       = a.FinalStatus?.ToString(),
        IsHealthy         = a.IsHealthy,
        Label             = a.Label,
        Confidence        = a.Confidence,
        GateLabel         = a.GateLabel,
        GateConfidence    = a.GateConfidence,
        DetectionCount    = a.DetectionCount,
        BestBox           = (a.BestBoxX1.HasValue && a.BestBoxY1.HasValue
                              && a.BestBoxX2.HasValue && a.BestBoxY2.HasValue)
                            ? new BoundingBoxDto
                            {
                                X1 = a.BestBoxX1.Value,
                                Y1 = a.BestBoxY1.Value,
                                X2 = a.BestBoxX2.Value,
                                Y2 = a.BestBoxY2.Value
                            }
                            : null,
        AnnotatedImageUrl = a.AnnotatedImageUrl,
        RawResultJson     = a.RawResultJson,
        ErrorMessage      = a.ErrorMessage,
        RequestedAt       = a.RequestedAt,
        CompletedAt       = a.CompletedAt,
        CreatedAt         = a.CreatedAt
    };

    // ------------------------------------------------------------------
    // Helper
    // ------------------------------------------------------------------

    private static AIProvider ChooseProvider(PlantImage image)
    {
        // Ưu tiên 1: FE đã chỉ định khi upload
        if (image.AIProvider.HasValue) return image.AIProvider.Value;

        // Ưu tiên 2: đoán từ caption
        var caption = image.Caption?.ToLowerInvariant() ?? "";
        if (caption.Contains("pest") || caption.Contains("sâu") || caption.Contains("côn trùng") || caption.Contains("insect"))
            return AIProvider.ArgoPestOnnx;

        // Mặc định: Tomato Leaf Disease
        return AIProvider.TomatoLeafDiseaseOnnx;
    }
}
