using System;
using System.Collections.Generic;

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

    public virtual Batch? Batch { get; set; }

    public virtual Experiment Experiment { get; set; } = null!;

    public virtual ICollection<PlantHealthAssessment> PlantHealthAssessments { get; set; } = new List<PlantHealthAssessment>();

    public virtual TaskReport? TaskReport { get; set; }

    public virtual User? UploadedByNavigation { get; set; }
}
