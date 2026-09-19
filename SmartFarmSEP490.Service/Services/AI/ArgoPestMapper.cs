using SmartFarmSEP490.Model;
using SmartFarmSEP490.Model.Enums;
using SmartFarmSEP490.Service.Interfaces.Commons;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SmartFarmSEP490.Service.Services.AI;

/// <summary>
/// Parse response từ Argo Pest API → AIAnalysis.
/// Đặc biệt: upload ảnh annotated (annotated_image_base64) lên Cloudinary.
/// </summary>
public static class ArgoPestMapper
{
    public const string ApiVersionString = "2.0.0";
    public const string AnnotatedFolder = "annotated";

    /// <summary>
    /// Parse + upload annotated image (nếu có) trả về tuple (AIAnalysis, AnnotatedUrl).
    /// </summary>
    public static async Task<(AIAnalysis analysis, string? annotatedUrl)> ParseAsync(
        PlantImage image,
        JsonElement root,
        ICloudinaryService cloudinary,
        CancellationToken ct = default)
    {
        if (image == null) throw new ArgumentNullException(nameof(image));
        if (root.ValueKind != JsonValueKind.Object)
            throw new ArgumentException("Argo Pest response must be a JSON object.", nameof(root));

        // ----- is_pest (required) -----
        var isPest = root.GetProperty("is_pest").GetBoolean();

        // ----- detections (required, có thể rỗng) -----
        var dets = root.GetProperty("detections");
        var detCount = dets.ValueKind == JsonValueKind.Array ? dets.GetArrayLength() : 0;

        // ----- Suy ra final_status (Argo Pest KHÔNG trả field này) -----
        AIFinalStatus finalStatus;
        string? label = null;
        decimal? confidence = null;
        decimal? boxX1 = null, boxY1 = null, boxX2 = null, boxY2 = null;

        if (!isPest)
        {
            finalStatus = AIFinalStatus.NoPest;
        }
        else if (detCount == 0)
        {
            finalStatus = AIFinalStatus.NoPestDetected;
        }
        else
        {
            finalStatus = AIFinalStatus.PestClassified;

            // Chọn detection có classification_confidence cao nhất (best detection)
            var best = dets.EnumerateArray()
                .OrderByDescending(d => d.GetProperty("classification_confidence").GetDouble())
                .First();

            label = best.GetProperty("class_name").GetString();
            confidence = (decimal)best.GetProperty("classification_confidence").GetDouble();

            // Bounding box: Tomato float array [x1,y1,x2,y2]; Argo Pest object {x1,y1,x2,y2} (int)
            if (best.TryGetProperty("box", out var boxEl) && boxEl.ValueKind == JsonValueKind.Object)
            {
                boxX1 = (decimal)boxEl.GetProperty("x1").GetDouble();
                boxY1 = (decimal)boxEl.GetProperty("y1").GetDouble();
                boxX2 = (decimal)boxEl.GetProperty("x2").GetDouble();
                boxY2 = (decimal)boxEl.GetProperty("y2").GetDouble();
            }
        }

        // ----- Gate label/confidence -----
        var gateLabel = isPest ? "pest" : "non_pest";
        decimal? gateConfidence = null;
        if (root.TryGetProperty("gate_confidence", out var gateConfEl) && gateConfEl.ValueKind == JsonValueKind.Object)
        {
            var key = isPest ? "pest" : "non_pest";
            if (gateConfEl.TryGetProperty(key, out var gc) && gc.ValueKind == JsonValueKind.Number)
                gateConfidence = (decimal)gc.GetDouble();
        }

        // ----- Annotated image upload (Argo Pest trả base64) -----
        string? annotatedUrl = null;
        if (root.TryGetProperty("annotated_image_base64", out var b64El)
            && b64El.ValueKind == JsonValueKind.String
            && !string.IsNullOrWhiteSpace(b64El.GetString()))
        {
            try
            {
                var bytes = Convert.FromBase64String(b64El.GetString()!);
                // Luôn upload — kể cả khi is_pest=false (Argo Pest vẫn trả ảnh gốc)
                annotatedUrl = await cloudinary.UploadBytesAsync(
                    bytes,
                    $"annotated_{image.Id}.jpg",
                    AnnotatedFolder,
                    ct);
            }
            catch (Exception ex)
            {
                // Upload fail không làm fail cả job — chỉ log
                // Worker sẽ ghi ErrorMessage riêng
                throw new InvalidOperationException($"Failed to upload annotated image: {ex.Message}", ex);
            }
        }

        var rawJson = root.GetRawText();

        var analysis = new AIAnalysis
        {
            Id                = Guid.NewGuid(),
            PlantImageId      = image.Id,
            TaskReportId      = image.TaskReportId, // null OK
            AIProvider        = AIProvider.ArgoPestOnnx,
            ApiVersion        = ApiVersionString,
            FinalStatus       = finalStatus,
            IsHealthy         = !isPest,
            Label             = label,
            Confidence        = confidence,
            GateLabel         = gateLabel,
            GateConfidence    = gateConfidence,
            DetectionCount    = detCount,
            BestBoxX1         = boxX1,
            BestBoxY1         = boxY1,
            BestBoxX2         = boxX2,
            BestBoxY2         = boxY2,
            RawResultJson     = rawJson,
            ProbabilitiesJson = null, // Argo Pest thường không trả probabilities
            AIStatus          = AIStatus.Completed,
            AnnotatedImageUrl = annotatedUrl,
            RequestedAt       = DateTime.UtcNow,
            CompletedAt       = DateTime.UtcNow
        };

        return (analysis, annotatedUrl);
    }
}
