using System;
using System.Collections.Generic;
using SmartFarmSEP490.Model.Enums;

namespace SmartFarmSEP490.Model;

public partial class ExperimentDesign
{
    public Guid Id { get; set; }

    public Guid ExperimentId { get; set; }

    public int? ReplicationCount { get; set; }

    public string? RandomizationMethod { get; set; }

    public string? DesignParameters { get; set; }

    public DesignType DesignType { get; set; } = DesignType.Other;

    public virtual Experiment Experiment { get; set; } = null!;
}
