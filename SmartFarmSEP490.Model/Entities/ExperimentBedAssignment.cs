using System;
using System.Collections.Generic;
using SmartFarmSEP490.Model.Enums;

namespace SmartFarmSEP490.Model;

public partial class ExperimentBedAssignment
{
    public Guid Id { get; set; }

    public Guid? RequestId { get; set; }

    public Guid? ExperimentId { get; set; }

    public Guid BedId { get; set; }

    public AllocationStatus Status { get; set; } = AllocationStatus.Reserved;

    public DateOnly AssignedFrom { get; set; }

    public DateOnly? AssignedTo { get; set; }

    public string? Purpose { get; set; }

    public virtual ICollection<Batch> Batches { get; set; } = new List<Batch>();

    public virtual Bed Bed { get; set; } = null!;

    public virtual Experiment? Experiment { get; set; }

    public virtual ExperimentRequest? Request { get; set; }
}
