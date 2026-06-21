using System;
using System.Collections.Generic;
using SmartFarmSEP490.Model.Enums;

namespace SmartFarmSEP490.Model;

public partial class Task
{
    public Guid Id { get; set; }

    public Guid ExperimentId { get; set; }

    public Guid? ExperimentStageId { get; set; }

    public Guid? BatchId { get; set; }

    public Guid? CareScheduleId { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid? AssignedTo { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public TaskType Type { get; set; } = TaskType.Other;

    public Enums.TaskStatus Status { get; set; } = Enums.TaskStatus.Pending;

    public string? RequiredSkillDescription { get; set; }

    public DateTime? DueDate { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<AitaskAssignmentSuggestion> AitaskAssignmentSuggestions { get; set; } = new List<AitaskAssignmentSuggestion>();

    public virtual User? AssignedToNavigation { get; set; }

    public virtual Batch? Batch { get; set; }

    public virtual CareSchedule? CareSchedule { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual Experiment Experiment { get; set; } = null!;

    public virtual ExperimentStage? ExperimentStage { get; set; }

    public virtual ICollection<TaskAssignment> TaskAssignments { get; set; } = new List<TaskAssignment>();

    public virtual ICollection<TaskReport> TaskReports { get; set; } = new List<TaskReport>();

    public virtual ICollection<TaskSkillRequirement> TaskSkillRequirements { get; set; } = new List<TaskSkillRequirement>();
}
