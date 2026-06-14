using System;
using System.Collections.Generic;

namespace SmartFarmSEP490.Model;

public partial class MeasurementDefinition
{
    public Guid Id { get; set; }

    public Guid ExperimentId { get; set; }

    public Guid? GroupId { get; set; }

    public string MetricName { get; set; } = null!;

    public string? Unit { get; set; }

    public decimal? TargetValue { get; set; }

    public string? Description { get; set; }

    public virtual Experiment Experiment { get; set; } = null!;

    public virtual ExperimentGroup? Group { get; set; }

    public virtual ICollection<MeasurementRecord> MeasurementRecords { get; set; } = new List<MeasurementRecord>();
}
