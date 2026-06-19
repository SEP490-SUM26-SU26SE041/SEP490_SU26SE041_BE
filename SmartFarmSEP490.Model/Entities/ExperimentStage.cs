using System;
using System.Collections.Generic;
using SmartFarmSEP490.Model.Enums;

namespace SmartFarmSEP490.Model;

public partial class ExperimentStage
{
    public Guid Id { get; set; }

    public Guid ExperimentId { get; set; }

    public string StageName { get; set; } = null!;

    public int StageOrder { get; set; }

    public string? Objective { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public string? ResultSummary { get; set; }

    public string? ResultData { get; set; }

    public ExperimentStageType StageType { get; set; } = ExperimentStageType.Other;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<CareSchedule> CareSchedules { get; set; } = new List<CareSchedule>();

    public virtual Experiment Experiment { get; set; } = null!;

    public virtual ICollection<MeasurementRecord> MeasurementRecords { get; set; } = new List<MeasurementRecord>();

    public virtual ICollection<Task> Tasks { get; set; } = new List<Task>();
}
