using System;
using System.Collections.Generic;
using SmartFarmSEP490.Model.Enums;

namespace SmartFarmSEP490.Model;

public partial class PlantImage
{
    public Guid Id { get; set; }

    public Guid ExperimentId { get; set; }

    public Guid? BatchId { get; set; }

    public Guid? TaskReportId { get; set; }

    public string ImageUrl { get; set; } = null!;

    public string? Caption { get; set; }

    public Guid? UploadedBy { get; set; }

    public DateTime? CapturedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    // ---- AI fields (denormalize từ AIAnalysis để FE poll nhanh) ----
    /// <summary>Trạng thái xử lý AI (Pending/Processing/Completed/Failed).</summary>
    public AIStatus AIStatus { get; set; } = AIStatus.Pending;

    /// <summary>AI provider đã chọn khi upload (null = Worker tự suy ra từ caption).</summary>
    public AIProvider? AIProvider { get; set; }

    /// <summary>Label AI predict (Tomato: tên bệnh; Argo: tên sâu). Copy nhanh từ AIAnalysis.Label.</summary>
    public string? AIPredictedLabel { get; set; }

    /// <summary>Confidence 0..1. Copy nhanh từ AIAnalysis.Confidence.</summary>
    public decimal? AIConfidence { get; set; }

    /// <summary>Confidence * 100 để FE hiển thị phần trăm (vd 91.0 thay vì 0.91).</summary>
    public decimal? AIConfidenceRate { get; set; }

    /// <summary>URL ảnh đã annotate (chỉ Argo Pest). Cloudinary URL.</summary>
    public string? AIAnnotatedImageUrl { get; set; }

    // ---- Navigation ----
    public virtual Batch? Batch { get; set; }

    public virtual Experiment Experiment { get; set; } = null!;

    public virtual ICollection<PlantHealthAssessment> PlantHealthAssessments { get; set; } = new List<PlantHealthAssessment>();

    public virtual ICollection<AIAnalysis> AIAnalyses { get; set; } = new List<AIAnalysis>();

    public virtual TaskReport? TaskReport { get; set; }

    public virtual User? UploadedByNavigation { get; set; }
}
