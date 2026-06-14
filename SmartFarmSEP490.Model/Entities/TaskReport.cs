using System;
using System.Collections.Generic;

namespace SmartFarmSEP490.Model;

public partial class TaskReport
{
    public Guid Id { get; set; }

    public Guid TaskId { get; set; }

    public Guid ReporterId { get; set; }

    public string? ReportText { get; set; }

    public string? ResultData { get; set; }

    public DateTime ReportedAt { get; set; }

    public virtual ICollection<PlantImage> PlantImages { get; set; } = new List<PlantImage>();

    public virtual User Reporter { get; set; } = null!;

    public virtual Task Task { get; set; } = null!;
}
