using System;
using System.Collections.Generic;

namespace SmartFarmSEP490.Model;

public partial class Farm
{
    public Guid Id { get; set; }

    public Guid? ManagerId { get; set; }

    public string FarmCode { get; set; } = null!;

    public string FarmName { get; set; } = null!;

    public string? Location { get; set; }

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual ICollection<Area> Areas { get; set; } = new List<Area>();

    public virtual ICollection<ExperimentRequest> ExperimentRequests { get; set; } = new List<ExperimentRequest>();

    public virtual ICollection<Experiment> Experiments { get; set; } = new List<Experiment>();

    public virtual User? Manager { get; set; }
}
