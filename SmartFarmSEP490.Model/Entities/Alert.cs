using System;
using System.Collections.Generic;

namespace SmartFarmSEP490.Model;

public partial class Alert
{
    public Guid Id { get; set; }

    public Guid? ExperimentId { get; set; }

    public Guid? SensorId { get; set; }

    public Guid? BatchId { get; set; }

    public string Title { get; set; } = null!;

    public string? Message { get; set; }

    public bool IsResolved { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ResolvedAt { get; set; }

    public virtual Batch? Batch { get; set; }

    public virtual Experiment? Experiment { get; set; }

    public virtual Sensor? Sensor { get; set; }
}
