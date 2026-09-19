using SmartFarmSEP490.Model;
using SmartFarmSEP490.Model.Enums;
using System;
using System.Text.Json;

namespace SmartFarmSEP490.Service.Services.AI;

/// <summary>
/// Parse response từ Tomato Leaf Disease ONNX API → AIAnalysis.
/// Mapping chi tiết xem <c>AI_APIS_INTEGRATION.md</c> §3.4 và §5.
/// </summary>
public static class TomatoMapper
{
    public const string ApiVersionString = "1.0.0";

    public static AIAnalysis Parse(PlantImage image, JsonElement root)
    {
        if (image == null) throw new ArgumentNullException(nameof(image));
        if (root.ValueKind != JsonValueKind.Object)
            throw new ArgumentException("Tomato response must be a JSON object.", nameof(root));

        // ----- final_status (required) -----
        var finalStatusStr = root.GetProperty("final_status").GetString()
            ?? throw new InvalidOperationException("Tomato response missing 'final_status'.");

        var finalStatus = MapFinalStatus(finalStatusStr);

        // ----- Label / Confidence (chỉ có khi classified) -----
        string? label = null;
        decimal? confidence = null;
        if (finalStatusStr == "tomato_leaf_classified")
        {
            label = root.TryGetProperty("best_disease_prediction", out var lblEl) && lblEl.ValueKind == JsonValueKind.String
                ? lblEl.GetString()
                : null;
            confidence = root.TryGetProperty("best_disease_confidence", out var confEl) && confEl.ValueKind == JsonValueKind.Number
                ? (decimal)confEl.GetDouble()
                : null;
        }

        // ----- Gate -----
        string? gateLabel = null;
        decimal? gateConfidence = null;
        if (root.TryGetProperty("gate", out var gateEl) && gateEl.ValueKind == JsonValueKind.Object)
        {
            gateLabel = gateEl.TryGetProperty("prediction", out var gl) && gl.ValueKind == JsonValueKind.String
                ? gl.GetString() : null;
            gateConfidence = gateEl.TryGetProperty("confidence", out var gc) && gc.ValueKind == JsonValueKind.Number
                ? (decimal)gc.GetDouble() : null;
        }

        // ----- Detection count + Best box -----
        var detCount = root.TryGetProperty("detections", out var detsEl) && detsEl.ValueKind == JsonValueKind.Array
            ? detsEl.GetArrayLength() : 0;

        decimal? boxX1 = null, boxY1 = null, boxX2 = null, boxY2 = null;

        // Best box = box_xyxy của crop_predictions[0] (top-1) — hoặc detections[0] nếu không có crop_predictions
        if (root.TryGetProperty("crop_predictions", out var cropsEl) && cropsEl.ValueKind == JsonValueKind.Array && cropsEl.GetArrayLength() > 0)
        {
            var firstCrop = cropsEl[0];
            if (firstCrop.TryGetProperty("box_xyxy", out var boxEl) && boxEl.ValueKind == JsonValueKind.Array && boxEl.GetArrayLength() == 4)
            {
                boxX1 = (decimal)boxEl[0].GetDouble();
                boxY1 = (decimal)boxEl[1].GetDouble();
                boxX2 = (decimal)boxEl[2].GetDouble();
                boxY2 = (decimal)boxEl[3].GetDouble();
            }
        }

        // ----- Probabilities (optional, nếu request có include_probabilities=true) -----
        string? probabilitiesJson = null;
        if (root.TryGetProperty("crop_predictions", out var cpEl) && cpEl.ValueKind == JsonValueKind.Array && cpEl.GetArrayLength() > 0)
        {
            var firstCrop = cpEl[0];
            if (firstCrop.TryGetProperty("probabilities", out var probEl) && probEl.ValueKind == JsonValueKind.Object)
            {
                probabilitiesJson = probEl.GetRawText();
            }
        }

        // ----- Raw JSON (lưu nguyên để debug) -----
        var rawJson = root.GetRawText();

        return new AIAnalysis
        {
            Id                = Guid.NewGuid(),
            PlantImageId      = image.Id,
            TaskReportId      = image.TaskReportId, // null OK nếu PlantImage chưa gắn TaskReport
            AIProvider        = AIProvider.TomatoLeafDiseaseOnnx,
            ApiVersion        = ApiVersionString,
            FinalStatus       = finalStatus,
            IsHealthy         = label == "Tomato_healthy" ? true : (label == null ? null : false),
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
            ProbabilitiesJson = probabilitiesJson,
            AIStatus          = AIStatus.Completed, // gọi thành công → Completed dù gate fail
            RequestedAt       = DateTime.UtcNow,
            CompletedAt       = DateTime.UtcNow
        };
    }

    /// <summary>Map final_status string (lowercase snake_case) sang enum AIFinalStatus.</summary>
    public static AIFinalStatus MapFinalStatus(string s) => s switch
    {
        "not_tomato_leaf"        => AIFinalStatus.NotTomatoLeaf,
        "no_leaf_detected"       => AIFinalStatus.NoLeafDetected,
        "tomato_leaf_classified" => AIFinalStatus.TomatoLeafClassified,
        _                        => AIFinalStatus.Unknown
    };
}
