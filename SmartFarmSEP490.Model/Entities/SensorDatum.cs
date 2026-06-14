using System;
using System.Collections.Generic;

namespace SmartFarmSEP490.Model;

public partial class SensorDatum
{
    public Guid Id { get; set; }

    public Guid SensorId { get; set; }

    public Guid? ExperimentId { get; set; }

    public Guid? BatchId { get; set; }

    public decimal Value { get; set; }

    public string? Unit { get; set; }

    public DateTime RecordedAt { get; set; }

    public virtual Batch? Batch { get; set; }

    public virtual Experiment? Experiment { get; set; }

    public virtual Sensor Sensor { get; set; } = null!;
}
