using System;
using System.Collections.Generic;

namespace SmartFarmSEP490.Model;

public partial class ExperimentGroup
{
    public Guid Id { get; set; }

    public Guid ExperimentId { get; set; }

    public string GroupName { get; set; } = null!;

    public string? TreatmentDescription { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Batch> Batches { get; set; } = new List<Batch>();

    public virtual Experiment Experiment { get; set; } = null!;

    public virtual ICollection<MeasurementDefinition> MeasurementDefinitions { get; set; } = new List<MeasurementDefinition>();
}
