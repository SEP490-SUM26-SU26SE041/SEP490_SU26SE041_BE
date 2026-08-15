using System;
using System.ComponentModel.DataAnnotations;

namespace SmartFarmSEP490.Model.DTOs;

// ============ Skill DTOs ============

public class SkillResponseDto
{
    public Guid Id { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public int TotalUsers { get; set; }
    public int TotalTasks { get; set; }
}

public class CreateSkillDto
{
    [Required, MaxLength(100)]
    public string SkillName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }
}

public class UpdateSkillDto
{
    [MaxLength(100)]
    public string? SkillName { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }
}

// ============ UserSkill DTOs ============

public class UserSkillResponseDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public Guid SkillId { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public int ProficiencyLevel { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateUserSkillDto
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    public Guid SkillId { get; set; }

    [Range(1, 10)]
    public int ProficiencyLevel { get; set; } = 1;

    [MaxLength(500)]
    public string? Description { get; set; }
}

public class UpdateUserSkillDto
{
    [Range(1, 10)]
    public int? ProficiencyLevel { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }
}

// ============ Task Count DTOs (theo ngày) ============

/// <summary>
/// Tổng số task trong ngày của 1 user có role Technician hoặc Student.
/// </summary>
public class UserTaskCountDto
{
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public int TotalTasks { get; set; }
    public int PendingTasks { get; set; }
    public int InProgressTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int OverdueTasks { get; set; }
    public int CancelledTasks { get; set; }
}

/// <summary>
/// Kết quả tổng hợp count task của mọi Technician và Student theo ngày.
/// </summary>
public class DailyTaskCountReportDto
{
    /// <summary>Ngày local (ICT) đã truyền vào.</summary>
    public DateOnly Date { get; set; }

    /// <summary>Cửa sổ UTC dùng để query (startUtc, endUtc).</summary>
    public DateTime StartUtc { get; set; }
    public DateTime EndUtc { get; set; }

    public int TotalUsers { get; set; }
    public int TotalTasks { get; set; }

    public int TechnicianTotal { get; set; }
    public int StudentTotal { get; set; }

    public List<UserTaskCountDto> Users { get; set; } = new();
}