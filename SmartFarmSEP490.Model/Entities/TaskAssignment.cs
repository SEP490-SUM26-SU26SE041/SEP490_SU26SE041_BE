using System;
using System.Collections.Generic;
using SmartFarmSEP490.Model.Enums;

namespace SmartFarmSEP490.Model;

public partial class TaskAssignment
{
    public Guid Id { get; set; }

    public Guid TaskId { get; set; }

    public Guid AssigneeId { get; set; }

    public Guid? AssignedBy { get; set; }

    public string? Reason { get; set; }

    public TaskAssignmentStatus Status { get; set; } = TaskAssignmentStatus.Assigned;

    public DateTime AssignedAt { get; set; }

    public DateTime? EndedAt { get; set; }

    public virtual User? AssignedByNavigation { get; set; }

    public virtual User Assignee { get; set; } = null!;

    public virtual Task Task { get; set; } = null!;
}
