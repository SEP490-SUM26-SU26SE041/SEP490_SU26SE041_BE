using System;
using System.Collections.Generic;

namespace SmartFarmSEP490.Model;

public partial class MeasurementRecord
{
    public Guid Id { get; set; }

    public Guid ExperimentId { get; set; }

    public Guid? ExperimentStageId { get; set; }

    public Guid BatchId { get; set; }

    public Guid? MeasurementDefinitionId { get; set; }

    public Guid? MeasuredBy { get; set; }

    public decimal? Value { get; set; }

    public string? TextValue { get; set; }

    public string? ExtraData { get; set; }

    public DateTime MeasuredAt { get; set; }

    public virtual Batch Batch { get; set; } = null!;

    public virtual Experiment Experiment { get; set; } = null!;

    public virtual ExperimentStage? ExperimentStage { get; set; }

    public virtual User? MeasuredByNavigation { get; set; }

    public virtual MeasurementDefinition? MeasurementDefinition { get; set; }
}
