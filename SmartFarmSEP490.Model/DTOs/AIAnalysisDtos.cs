using System;
using System.Collections.Generic;

namespace SmartFarmSEP490.Model.DTOs;

/// <summary>
/// DTO trả về cho FE khi poll <c>GET /api/task-images/task/{taskReportId}</c>.
/// Gồm các field flat từ PlantImage (cho list view) + optional AIAnalysis chi tiết.
/// </summary>
public class PlantImageWithAnalysisDto
{
    public Guid Id { get; set; }

    public Guid? TaskReportId { get; set; }

    public Guid? BatchId { get; set; }

    public Guid ExperimentId { get; set; }

    public string ImageUrl { get; set; } = null!;

    public string? Caption { get; set; }

    public DateTime? CapturedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    // ---- AI flat fields (denormalize) ----
    public string AIStatus { get; set; } = "Pending";

    public string? AIProvider { get; set; }

    public string? AIPredictedLabel { get; set; }

    public decimal? AIConfidence { get; set; }

    public decimal? AIConfidenceRate { get; set; }

    public string? AIAnnotatedImageUrl { get; set; }

    /// <summary>Chi tiết AI analysis (optional - chỉ khi ?includeAnalysis=true).</summary>
    public AIAnalysisDto? AIAnalysis { get; set; }
}

public class AIAnalysisDto
{
    public Guid Id { get; set; }

    public Guid PlantImageId { get; set; }

    public Guid? TaskReportId { get; set; }

    public string AIProvider { get; set; } = null!;

    public string? ApiVersion { get; set; }

    public string? FinalStatus { get; set; }

    public bool? IsHealthy { get; set; }

    public string? Label { get; set; }

    public decimal? Confidence { get; set; }

    public string? GateLabel { get; set; }

    public decimal? GateConfidence { get; set; }

    public int DetectionCount { get; set; }

    public BoundingBoxDto? BestBox { get; set; }

    public string? AnnotatedImageUrl { get; set; }

    /// <summary>Toàn bộ raw JSON response từ AI API (giữ nguyên format, dùng để debug + FE hiển thị chi tiết).</summary>
    public string? RawResultJson { get; set; }

    public string? ErrorMessage { get; set; }

    public DateTime RequestedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public DateTime CreatedAt { get; set; }
}

public class BoundingBoxDto
{
    public decimal X1 { get; set; }
    public decimal Y1 { get; set; }
    public decimal X2 { get; set; }
    public decimal Y2 { get; set; }
}
