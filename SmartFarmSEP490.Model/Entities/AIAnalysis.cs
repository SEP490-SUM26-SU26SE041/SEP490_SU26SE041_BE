using System;
using System.Collections.Generic;
using SmartFarmSEP490.Model.Enums;

namespace SmartFarmSEP490.Model;

/// <summary>
/// Kết quả phân tích AI cho một PlantImage (quan hệ 1–1).
/// Lưu toàn bộ raw JSON để debug + các field structured để query.
/// Mỗi PlantImage chỉ có tối đa 1 AIAnalysis.
/// </summary>
public partial class AIAnalysis
{
    public Guid Id { get; set; }

    public Guid PlantImageId { get; set; }

    /// <summary>FK denormalize từ PlantImage.TaskReportId để truy vấn nhanh. Có thể null nếu PlantImage chưa gắn TaskReport.</summary>
    public Guid? TaskReportId { get; set; }

    // ---- Nguồn AI ----
    public AIProvider AIProvider { get; set; }

    public string? ApiVersion { get; set; }

    // ---- Kết quả phân loại ----
    public AIFinalStatus? FinalStatus { get; set; }

    /// <summary>
    /// true nếu cây không có bệnh / không có sâu.
    /// - Tomato: best_disease_prediction == "Tomato_healthy"
    /// - Argo Pest: !is_pest
    /// Note: <c>null</c> khi không xác định được (vd gate pass nhưng không có detection).
    /// </summary>
    public bool? IsHealthy { get; set; }

    /// <summary>Tên bệnh (Tomato) hoặc tên loài sâu (Argo Pest). null khi không classify được.</summary>
    public string? Label { get; set; }

    /// <summary>Độ tin cậy 0..1.</summary>
    public decimal? Confidence { get; set; }

    // ---- Gate (cả 2 AI đều có gate) ----
    public string? GateLabel { get; set; }

    public decimal? GateConfidence { get; set; }

    // ---- Detection ----
    public int DetectionCount { get; set; }

    /// <summary>Bounding-box tốt nhất (pixel gốc). Tomato trả float, Argo Pest trả int → lưu numeric.</summary>
    public decimal? BestBoxX1 { get; set; }

    public decimal? BestBoxY1 { get; set; }

    public decimal? BestBoxX2 { get; set; }

    public decimal? BestBoxY2 { get; set; }

    /// <summary>Toàn bộ JSON response từ AI. Dùng để debug và FE hiển thị chi tiết.</summary>
    public string? RawResultJson { get; set; }

    /// <summary>Probabilities object (Tomato: map bệnh→conf; Argo: thường null).</summary>
    public string? ProbabilitiesJson { get; set; }

    /// <summary>Trạng thái xử lý AI của bản ghi này.</summary>
    public AIStatus AIStatus { get; set; } = AIStatus.Pending;

    /// <summary>
    /// URL ảnh đã annotate (chỉ Argo Pest mới có).
    /// Worker sẽ decode base64 → upload Cloudinary → lưu URL.
    /// Tomato thì để null.
    /// </summary>
    public string? AnnotatedImageUrl { get; set; }

    /// <summary>Lỗi nếu có (4xx/5xx/timeout/parse error).</summary>
    public string? ErrorMessage { get; set; }

    // ---- Timestamps ----
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

    public DateTime? CompletedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // ---- Navigation ----
    public virtual PlantImage PlantImage { get; set; } = null!;

    public virtual TaskReport? TaskReport { get; set; }
}
