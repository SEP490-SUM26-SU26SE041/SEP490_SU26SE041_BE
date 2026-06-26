using System;
using System.Collections.Generic;

namespace SmartFarmSEP490.Model;

public partial class Bed
{
    public Guid Id { get; set; }

    public Guid AreaId { get; set; }

    public string BedCode { get; set; } = null!;

    public string? SoilDescription { get; set; }

    public decimal? Length { get; set; }

    public decimal? Width { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual Area Area { get; set; } = null!;

    public virtual ICollection<ExperimentBedAssignment> ExperimentBedAssignments { get; set; } = new List<ExperimentBedAssignment>();
}
