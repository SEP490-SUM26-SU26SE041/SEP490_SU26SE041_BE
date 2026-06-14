using System;
using System.Collections.Generic;

namespace SmartFarmSEP490.Model;

public partial class ExperimentReport
{
    public Guid Id { get; set; }

    public Guid ExperimentId { get; set; }

    public Guid? CreatedBy { get; set; }

    public string ReportType { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string? Summary { get; set; }

    public string? ResultData { get; set; }

    public string? ExportFormat { get; set; }

    public string? FileUrl { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual Experiment Experiment { get; set; } = null!;
}
