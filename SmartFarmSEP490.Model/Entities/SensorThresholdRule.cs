using System;
using System.Collections.Generic;

namespace SmartFarmSEP490.Model;

public partial class SensorThresholdRule
{
    public Guid Id { get; set; }

    public Guid ExperimentId { get; set; }

    public Guid? BatchId { get; set; }

    public decimal? MinValue { get; set; }

    public decimal? MaxValue { get; set; }

    public string? Message { get; set; }

    public bool IsActive { get; set; }

    public virtual Batch? Batch { get; set; }

    public virtual Experiment Experiment { get; set; } = null!;
}
