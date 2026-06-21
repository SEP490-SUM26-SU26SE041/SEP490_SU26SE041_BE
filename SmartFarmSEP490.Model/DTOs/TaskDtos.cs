namespace SmartFarmSEP490.Model.DTOs;

// ============ Task DTOs ============

public class CreateTaskDto
{
    public Guid ExperimentId { get; set; }
    public Guid? ExperimentStageId { get; set; }
    public Guid? BatchId { get; set; }
    public Guid? CareScheduleId { get; set; }
    public string TaskType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? RequiredSkillDescription { get; set; }
    public DateTime? DueDate { get; set; }
}

public class UpdateTaskDto
{
    public Guid? ExperimentStageId { get; set; }
    public Guid? BatchId { get; set; }
    public Guid? CareScheduleId { get; set; }
    public string? TaskType { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? RequiredSkillDescription { get; set; }
    public DateTime? DueDate { get; set; }
    public string? Status { get; set; }
}

public class TaskResponseDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string TaskType { get; set; } = string.Empty;
    public string? RequiredSkillDescription { get; set; }
    public DateTime? DueDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Guid ExperimentId { get; set; }
    public string? ExperimentTitle { get; set; }
    public string? ExperimentCode { get; set; }

    public Guid? ExperimentStageId { get; set; }
    public string? ExperimentStageName { get; set; }

    public Guid? BatchId { get; set; }
    public string? BatchCode { get; set; }

    public Guid? CareScheduleId { get; set; }
    public string? CareScheduleTitle { get; set; }

    public Guid? CreatedBy { get; set; }
    public string? CreatedByName { get; set; }

    public Guid? AssignedTo { get; set; }
    public string? AssignedToName { get; set; }

    public List<TaskSkillRequirementResponseDto> SkillRequirements { get; set; } = new();
    public List<TaskAssignmentResponseDto> Assignments { get; set; } = new();
}

public class TaskSkillRequirementResponseDto
{
    public Guid SkillId { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public int RequiredLevel { get; set; }
}

// ============ TaskAssignment DTOs ============

public class AssignTaskDto
{
    public Guid TaskId { get; set; }
    public Guid AssigneeId { get; set; }
    public string? Reason { get; set; }
}

public class ReassignTaskDto
{
    public Guid TaskId { get; set; }
    public Guid NewAssigneeId { get; set; }
    public string? Reason { get; set; }
}

public class UpdateTaskAssignmentStatusDto
{
    public Guid AssignmentId { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class TaskAssignmentResponseDto
{
    public Guid Id { get; set; }
    public Guid TaskId { get; set; }
    public string TaskTitle { get; set; } = string.Empty;
    public Guid AssigneeId { get; set; }
    public string AssigneeName { get; set; } = string.Empty;
    public string AssigneeEmail { get; set; } = string.Empty;
    public string AssigneeRole { get; set; } = string.Empty;
    public List<AssigneeSkillDto> AssigneeSkills { get; set; } = new();
    public Guid? AssignedBy { get; set; }
    public string? AssignedByName { get; set; }
    public string? Reason { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime AssignedAt { get; set; }
    public DateTime? EndedAt { get; set; }
}

public class AssigneeSkillDto
{
    public Guid SkillId { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public int ProficiencyLevel { get; set; }
}

// ============ Skill Matching DTOs ============

public class SkillMatchResultDto
{
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int MatchScore { get; set; }
    public List<UserMatchedSkillDto> MatchedSkills { get; set; } = new();
    public List<UserMissingSkillDto> MissingSkills { get; set; } = new();
}

public class UserMatchedSkillDto
{
    public Guid SkillId { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public int RequiredLevel { get; set; }
    public int UserLevel { get; set; }
}

public class UserMissingSkillDto
{
    public Guid SkillId { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public int RequiredLevel { get; set; }
}
