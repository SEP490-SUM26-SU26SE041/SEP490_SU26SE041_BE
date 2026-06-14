using System;
using System.Collections.Generic;

namespace SmartFarmSEP490.Model;

public partial class CareSchedule
{
    public Guid Id { get; set; }

    public Guid ExperimentId { get; set; }

    public Guid? ExperimentStageId { get; set; }

    public Guid? BatchId { get; set; }

    public string Title { get; set; } = null!;

    public string? Instruction { get; set; }

    public int? FrequencyDays { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Batch? Batch { get; set; }

    public virtual Experiment Experiment { get; set; } = null!;

    public virtual ExperimentStage? ExperimentStage { get; set; }

    public virtual ICollection<Task> Tasks { get; set; } = new List<Task>();
}
