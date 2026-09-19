using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SmartFarmSEP490.Model;
using SmartFarmSEP490.Model.DTOs;
using SmartFarmSEP490.Model.Enums;
using SmartFarmSEP490.Repository.Interfaces.Tasks;
using SmartFarmSEP490.Service.Interfaces.AI;
using SmartFarmSEP490.Service.Interfaces.Commons;
using SmartFarmSEP490.Service.Interfaces.Tasks;

namespace SmartFarmSEP490.Service.Services.Tasks;

public class TaskImageService : ITaskImageService
{
    private readonly IPlantImageRepository _imageRepository;
    private readonly ICloudinaryService _cloudinaryService;
    private readonly IAIAnalysisService _aiService;
    private readonly ILogger<TaskImageService> _logger;

    public TaskImageService(
        IPlantImageRepository imageRepository,
        ICloudinaryService cloudinaryService,
        IAIAnalysisService aiService,
        ILogger<TaskImageService> logger)
    {
        _imageRepository = imageRepository;
        _cloudinaryService = cloudinaryService;
        _aiService = aiService;
        _logger = logger;
    }

    public async System.Threading.Tasks.Task<PlantImageResponseDto?> UploadAsync(
        IFormFile file,
        Guid experimentId,
        Guid? batchId,
        Guid? taskReportId,
        string? caption,
        DateTime? capturedAt,
        string? aiProvider,
        Guid uploadedBy,
        CancellationToken ct = default)
    {
        if (file == null || file.Length == 0)
            return null;

        // 1. Push file bytes to Cloudinary, receive a hosted imageUrl
        var imageUrl = await _cloudinaryService.UploadImageAsync(file, "smartfarm/task-images", ct);

        // 2. Insert PlantImage (status = Pending)
        var image = new PlantImage
        {
            Id = Guid.NewGuid(),
            ExperimentId = experimentId,
            BatchId = batchId,
            TaskReportId = taskReportId,
            ImageUrl = imageUrl,
            Caption = caption,
            UploadedBy = uploadedBy,
            CapturedAt = capturedAt,
            CreatedAt = DateTime.UtcNow,
            AIStatus = AIStatus.Pending,
            // Lưu provider FE chọn để Worker biết dùng model nào.
            // Null → Worker tự suy ra từ caption.
            AIProvider = ParseProviderOrNull(aiProvider)
        };

        await _imageRepository.CreateAsync(image);

        // 3. Enqueue AI Worker (không block request — channel write là O(1))
        //    Worker sẽ gọi AI → parse → update PlantImage + insert AIAnalysis
        try
        {
            await _aiService.EnqueueAsync(image.Id, ct);
        }
        catch (Exception ex)
        {
            // Enqueue fail không làm fail upload — chỉ log để debug.
            // Có thể có batch script quét PlantImage.AIStatus='Pending' sau để retry.
            _logger.LogError(ex,
                "[TaskImage] Failed to enqueue AI for PlantImage {Id}. Will need retry.",
                image.Id);
        }

        return await MapToResponseDto(image);
    }

    public async System.Threading.Tasks.Task<List<PlantImageResponseDto>> GetByTaskReportIdAsync(Guid taskReportId)
    {
        var images = await _imageRepository.GetByTaskReportIdAsync(taskReportId);
        var results = new List<PlantImageResponseDto>();
        foreach (var i in images) results.Add(await MapToResponseDto(i));
        return results;
    }

    public async System.Threading.Tasks.Task<List<PlantImageResponseDto>> GetByBatchIdAsync(Guid batchId)
    {
        var images = await _imageRepository.GetByBatchIdAsync(batchId);
        var results = new List<PlantImageResponseDto>();
        foreach (var i in images) results.Add(await MapToResponseDto(i));
        return results;
    }

    public async System.Threading.Tasks.Task<bool> DeleteAsync(Guid id, Guid userId)
    {
        var image = await _imageRepository.GetByIdAsync(id);
        if (image == null) return false;

        await _imageRepository.DeleteAsync(id);
        return true;
    }

    private async System.Threading.Tasks.Task<PlantImageResponseDto> MapToResponseDto(PlantImage image)
    {
        return new PlantImageResponseDto
        {
            Id = image.Id,
            ExperimentId = image.ExperimentId,
            BatchId = image.BatchId,
            BatchCode = image.Batch?.BatchCode,
            TaskReportId = image.TaskReportId,
            ImageUrl = image.ImageUrl,
            Caption = image.Caption,
            UploadedBy = image.UploadedBy,
            UploadedByName = image.UploadedByNavigation?.FullName,
            CapturedAt = image.CapturedAt,
            CreatedAt = image.CreatedAt,
            AIStatus = image.AIStatus.ToString(),
            AIProvider = image.AIProvider?.ToString(),
            AIPredictedLabel = image.AIPredictedLabel,
            AIConfidence = image.AIConfidence,
            AIConfidenceRate = image.AIConfidenceRate,
            AIAnnotatedImageUrl = image.AIAnnotatedImageUrl
        };
    }

    /// <summary>Parse AIProvider string từ form sang enum.</summary>
    private static AIProvider? ParseProviderOrNull(string? providerStr)
    {
        if (string.IsNullOrWhiteSpace(providerStr)) return null;
        if (Enum.TryParse<AIProvider>(providerStr, ignoreCase: true, out var result))
            return result;
        return null;
    }
}
