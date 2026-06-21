using System;
using System.Collections.Generic;
using SmartFarmSEP490.Model.Enums;

namespace SmartFarmSEP490.Model;

public partial class Area
{
    public Guid Id { get; set; }

    public Guid FarmId { get; set; }

    public string AreaCode { get; set; } = null!;

    public string AreaName { get; set; } = null!;

    public string? EnvironmentType { get; set; }

    public LocationStatus Status { get; set; } = LocationStatus.Available;

    public decimal? TotalArea { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual ICollection<Bed> Beds { get; set; } = new List<Bed>();

    public virtual Farm Farm { get; set; } = null!;
}
