using System;
using System.Collections.Generic;
using SmartFarmSEP490.Model.Enums;

namespace SmartFarmSEP490.Model;

public partial class ExperimentGroup
{
    public Guid Id { get; set; }

    public Guid ExperimentId { get; set; }

    public string GroupName { get; set; } = null!;

    public string? TreatmentDescription { get; set; }

    public GroupType GroupType { get; set; } = GroupType.Control;

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Batch> Batches { get; set; } = new List<Batch>();

    public virtual Experiment Experiment { get; set; } = null!;

    public virtual ICollection<MeasurementDefinition> MeasurementDefinitions { get; set; } = new List<MeasurementDefinition>();
}
