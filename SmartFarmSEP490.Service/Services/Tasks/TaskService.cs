using SmartFarmSEP490.Model.DTOs;
using SmartFarmSEP490.Repository.DbContexts;
using SmartFarmSEP490.Repository.Interfaces.Auth;
using SmartFarmSEP490.Repository.Interfaces.Experiments;
using SmartFarmSEP490.Repository.Interfaces.Tasks;
using SmartFarmSEP490.Service.Interfaces.Tasks;

namespace SmartFarmSEP490.Service.Services.Tasks;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _taskRepository;
    private readonly ITaskAssignmentRepository _assignmentRepository;
    private readonly ITaskSkillRequirementRepository _skillRequirementRepository;
    private readonly IExperimentRepository _experimentRepository;
    private readonly IUserRepository _userRepository;
    private readonly SmartFarmDbContext _context;

    private static readonly string[] AssignableRoles = { "Technician", "Student" };

    public TaskService(
        ITaskRepository taskRepository,
        ITaskAssignmentRepository assignmentRepository,
        ITaskSkillRequirementRepository skillRequirementRepository,
        IExperimentRepository experimentRepository,
        IUserRepository userRepository,
        SmartFarmDbContext context)
    {
        _taskRepository = taskRepository;
        _assignmentRepository = assignmentRepository;
        _skillRequirementRepository = skillRequirementRepository;
        _experimentRepository = experimentRepository;
        _userRepository = userRepository;
        _context = context;
    }

    public async System.Threading.Tasks.Task<bool> ValidateUserRoleAsync(Guid userId, params string[] allowedRoles)
    {
        var user = await _userRepository.GetUserByIdAsync(userId);
        if (user == null) return false;
        return user.UserRoles.Any(ur => allowedRoles.Contains(ur.Role.RoleName));
    }

    public async System.Threading.Tasks.Task<TaskResponseDto?> CreateAsync(CreateTaskDto dto, Guid createdById)
    {
        var experiment = await _experimentRepository.GetByIdAsync(dto.ExperimentId);
        if (experiment == null) return null!;

        var task = new SmartFarmSEP490.Model.Task
        {
            Id = Guid.NewGuid(),
            ExperimentId = dto.ExperimentId,
            ExperimentStageId = dto.ExperimentStageId,
            BatchId = dto.BatchId,
            CareScheduleId = dto.CareScheduleId,
            CreatedBy = createdById,
            Type = Enum.TryParse<SmartFarmSEP490.Model.Enums.TaskType>(dto.TaskType, ignoreCase: true, out var tt) ? tt : SmartFarmSEP490.Model.Enums.TaskType.Other,
            Title = dto.Title,
            Description = dto.Description,
            RequiredSkillDescription = dto.RequiredSkillDescription,
            DueDate = dto.DueDate,
            Status = SmartFarmSEP490.Model.Enums.TaskStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _taskRepository.AddAsync(task);
        return await MapToResponseDto(task);
    }

    public async System.Threading.Tasks.Task<TaskResponseDto?> GetByIdAsync(Guid id)
    {
        var task = await _taskRepository.GetByIdAsync(id);
        return task == null ? null! : await MapToResponseDto(task);
    }

    public async System.Threading.Tasks.Task<List<TaskResponseDto>> GetByExperimentAsync(Guid experimentId)
    {
        var tasks = await _taskRepository.GetByExperimentAsync(experimentId);
        var results = new List<TaskResponseDto>();
        foreach (var t in tasks) results.Add(await MapToResponseDto(t));
        return results;
    }

    public async System.Threading.Tasks.Task<List<TaskResponseDto>> GetByAssigneeAsync(Guid assigneeId)
    {
        var tasks = await _taskRepository.GetByAssigneeAsync(assigneeId);
        var results = new List<TaskResponseDto>();
        foreach (var t in tasks) results.Add(await MapToResponseDto(t));
        return results;
    }

    public async System.Threading.Tasks.Task<List<TaskResponseDto>> GetAllAsync()
    {
        var tasks = await _taskRepository.GetAllAsync();
        var results = new List<TaskResponseDto>();
        foreach (var t in tasks) results.Add(await MapToResponseDto(t));
        return results;
    }

    public async System.Threading.Tasks.Task<TaskResponseDto?> UpdateAsync(Guid id, UpdateTaskDto dto, Guid userId)
    {
        var task = await _taskRepository.GetByIdAsync(id);
        if (task == null) return null!;

        if (dto.ExperimentStageId != null) task.ExperimentStageId = dto.ExperimentStageId;
        if (dto.BatchId != null) task.BatchId = dto.BatchId;
        if (dto.CareScheduleId != null) task.CareScheduleId = dto.CareScheduleId;
        if (!string.IsNullOrEmpty(dto.TaskType)) task.Type = Enum.TryParse<SmartFarmSEP490.Model.Enums.TaskType>(dto.TaskType, ignoreCase: true, out var tt) ? tt : task.Type;
        if (!string.IsNullOrEmpty(dto.Title)) task.Title = dto.Title;
        if (dto.Description != null) task.Description = dto.Description;
        if (dto.RequiredSkillDescription != null) task.RequiredSkillDescription = dto.RequiredSkillDescription;
        if (dto.DueDate.HasValue) task.DueDate = dto.DueDate;
        if (!string.IsNullOrEmpty(dto.Status)) task.Status = Enum.TryParse<SmartFarmSEP490.Model.Enums.TaskStatus>(dto.Status, ignoreCase: true, out var ts) ? ts : task.Status;
        task.UpdatedAt = DateTime.UtcNow;

        await _taskRepository.UpdateAsync(task);
        return await MapToResponseDto(task);
    }

    public async System.Threading.Tasks.Task<bool> DeleteAsync(Guid id)
    {
        var task = await _taskRepository.GetByIdAsync(id);
        if (task == null) return false;
        await _taskRepository.DeleteAsync(id);
        return true;
    }

    public async System.Threading.Tasks.Task<TaskResponseDto?> AssignTaskAsync(AssignTaskDto dto, Guid assignedById)
    {
        var task = await _taskRepository.GetByIdAsync(dto.TaskId);
        if (task == null) return null!;

        var assignee = await _userRepository.GetUserByIdAsync(dto.AssigneeId);
        if (assignee == null) return null!;

        var isAllowedRole = assignee.UserRoles.Any(ur => AssignableRoles.Contains(ur.Role.RoleName));
        if (!isAllowedRole) return null!;

        var existingActive = await _assignmentRepository.GetActiveByTaskAndAssigneeAsync(dto.TaskId, dto.AssigneeId);
        if (existingActive != null) return null!;

        var assignment = new SmartFarmSEP490.Model.TaskAssignment
        {
            Id = Guid.NewGuid(),
            TaskId = dto.TaskId,
            AssigneeId = dto.AssigneeId,
            AssignedBy = assignedById,
            Status = SmartFarmSEP490.Model.Enums.TaskAssignmentStatus.Assigned,
            Reason = dto.Reason,
            AssignedAt = DateTime.UtcNow
        };

        await _assignmentRepository.AddAsync(assignment);

        task.AssignedTo = dto.AssigneeId;
        task.UpdatedAt = DateTime.UtcNow;
        await _taskRepository.UpdateAsync(task);

        return await MapToResponseDto(task);
    }

    public async System.Threading.Tasks.Task<TaskResponseDto?> ReassignTaskAsync(ReassignTaskDto dto, Guid reassignedById)
    {
        var task = await _taskRepository.GetByIdAsync(dto.TaskId);
        if (task == null) return null!;

        var newAssignee = await _userRepository.GetUserByIdAsync(dto.NewAssigneeId);
        if (newAssignee == null) return null!;

        var isAllowedRole = newAssignee.UserRoles.Any(ur => AssignableRoles.Contains(ur.Role.RoleName));
        if (!isAllowedRole) return null!;

        var existingAssignments = await _assignmentRepository.GetByTaskIdAsync(dto.TaskId);
        foreach (var existing in existingAssignments.Where(a => a.EndedAt == null))
        {
            existing.Status = SmartFarmSEP490.Model.Enums.TaskAssignmentStatus.Reassigned;
            existing.EndedAt = DateTime.UtcNow;
            await _assignmentRepository.UpdateAsync(existing);
        }

        var newAssignment = new SmartFarmSEP490.Model.TaskAssignment
        {
            Id = Guid.NewGuid(),
            TaskId = dto.TaskId,
            AssigneeId = dto.NewAssigneeId,
            AssignedBy = reassignedById,
            Status = SmartFarmSEP490.Model.Enums.TaskAssignmentStatus.Assigned,
            Reason = dto.Reason,
            AssignedAt = DateTime.UtcNow
        };

        await _assignmentRepository.AddAsync(newAssignment);

        task.AssignedTo = dto.NewAssigneeId;
        task.UpdatedAt = DateTime.UtcNow;
        await _taskRepository.UpdateAsync(task);

        return await MapToResponseDto(task);
    }

    public async System.Threading.Tasks.Task<TaskResponseDto?> UpdateTaskStatusAsync(Guid id, string status, Guid userId)
    {
        var task = await _taskRepository.GetByIdAsync(id);
        if (task == null) return null!;

        task.Status = Enum.TryParse<SmartFarmSEP490.Model.Enums.TaskStatus>(status, ignoreCase: true, out var ts) ? ts : task.Status;
        task.UpdatedAt = DateTime.UtcNow;
        await _taskRepository.UpdateAsync(task);

        return await MapToResponseDto(task);
    }

    public async System.Threading.Tasks.Task<TaskAssignmentResponseDto?> UpdateAssignmentStatusAsync(UpdateTaskAssignmentStatusDto dto)
    {
        var assignment = await _assignmentRepository.GetByIdAsync(dto.AssignmentId);
        if (assignment == null) return null!;

        assignment.Status = Enum.TryParse<SmartFarmSEP490.Model.Enums.TaskAssignmentStatus>(dto.Status, ignoreCase: true, out var as_) ? as_ : assignment.Status;
        if (dto.Status == "Completed" || dto.Status == "Cancelled" || dto.Status == "Resigned")
        {
            assignment.EndedAt = DateTime.UtcNow;
        }
        await _assignmentRepository.UpdateAsync(assignment);

        if (dto.Status == "Completed")
        {
            var task = await _taskRepository.GetByIdAsync(assignment.TaskId);
            if (task != null)
            {
                task.Status = SmartFarmSEP490.Model.Enums.TaskStatus.Completed;
                task.UpdatedAt = DateTime.UtcNow;
                await _taskRepository.UpdateAsync(task);
            }
        }

        return MapAssignmentToResponseDto(assignment);
    }

    public async System.Threading.Tasks.Task<List<TaskAssignmentResponseDto>> GetTaskAssignmentsAsync(Guid taskId)
    {
        var assignments = await _assignmentRepository.GetByTaskIdAsync(taskId);
        return assignments.Select(MapAssignmentToResponseDto).ToList();
    }

    public async System.Threading.Tasks.Task<List<TaskAssignmentResponseDto>> GetAssignmentsByAssigneeAsync(Guid assigneeId)
    {
        var assignments = await _assignmentRepository.GetByAssigneeAsync(assigneeId);
        return assignments.Select(MapAssignmentToResponseDto).ToList();
    }

    public async System.Threading.Tasks.Task<List<SkillMatchResultDto>> FindMatchingUsersAsync(Guid taskId)
    {
        var task = await _taskRepository.GetByIdAsync(taskId);
        if (task == null) return new List<SkillMatchResultDto>();

        var skillRequirements = await _skillRequirementRepository.GetByTaskAsync(taskId);
        var eligibleUsers = await _skillRequirementRepository.GetUsersWithSkillsAsync(
            AssignableRoles.ToList(), skillRequirements.Select(s => s.SkillId).ToList());

        var results = new List<SkillMatchResultDto>();

        foreach (var user in eligibleUsers)
        {
            var matchedSkills = new List<UserMatchedSkillDto>();
            var missingSkills = new List<UserMissingSkillDto>();
            int matchedCount = 0;
            int totalRequired = skillRequirements.Count;

            foreach (var req in skillRequirements)
            {
                var userSkill = user.UserSkills.FirstOrDefault(us => us.SkillId == req.SkillId);
                if (userSkill != null && userSkill.ProficiencyLevel >= req.RequiredLevel)
                {
                    matchedCount++;
                    matchedSkills.Add(new UserMatchedSkillDto
                    {
                        SkillId = req.SkillId,
                        SkillName = req.Skill.SkillName,
                        RequiredLevel = req.RequiredLevel,
                        UserLevel = userSkill.ProficiencyLevel
                    });
                }
                else
                {
                    missingSkills.Add(new UserMissingSkillDto
                    {
                        SkillId = req.SkillId,
                        SkillName = req.Skill.SkillName,
                        RequiredLevel = req.RequiredLevel
                    });
                }
            }

            if (totalRequired == 0) matchedCount = 1;

            int score = totalRequired == 0 ? 100 : (matchedCount * 100) / totalRequired;

            results.Add(new SkillMatchResultDto
            {
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                RoleName = user.UserRoles.FirstOrDefault()?.Role?.RoleName ?? "",
                IsActive = user.IsActive,
                MatchScore = score,
                MatchedSkills = matchedSkills,
                MissingSkills = missingSkills
            });
        }

        return results.OrderByDescending(r => r.MatchScore).ToList();
    }

    private async System.Threading.Tasks.Task<TaskResponseDto> MapToResponseDto(SmartFarmSEP490.Model.Task task)
    {
        var experiment = await _experimentRepository.GetByIdAsync(task.ExperimentId);
        string? experimentTitle = experiment?.Title;
        string? experimentCode = experiment?.ExperimentCode;

        string? experimentStageName = task.ExperimentStageId.HasValue ? task.ExperimentStage?.StageName : null;
        string? batchCode = task.BatchId.HasValue ? task.Batch?.BatchCode : null;
        string? careScheduleTitle = task.CareScheduleId.HasValue ? task.CareSchedule?.Title : null;
        string? createdByName = task.CreatedByNavigation?.FullName;
        string? assignedToName = task.AssignedToNavigation?.FullName;

        var skillReqDtos = task.TaskSkillRequirements
            .Select(tsr => new TaskSkillRequirementResponseDto
            {
                SkillId = tsr.SkillId,
                SkillName = tsr.Skill.SkillName,
                RequiredLevel = tsr.RequiredLevel
            }).ToList();

        var assignmentDtos = task.TaskAssignments
            .Select(MapAssignmentToResponseDto)
            .ToList();

        return new TaskResponseDto
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            TaskType = task.Type.ToString(),
            RequiredSkillDescription = task.RequiredSkillDescription,
            DueDate = task.DueDate,
            Status = task.Status.ToString(),
            CreatedAt = task.CreatedAt,
            UpdatedAt = task.UpdatedAt,
            ExperimentId = task.ExperimentId,
            ExperimentTitle = experimentTitle,
            ExperimentCode = experimentCode,
            ExperimentStageId = task.ExperimentStageId,
            ExperimentStageName = experimentStageName,
            BatchId = task.BatchId,
            BatchCode = batchCode,
            CareScheduleId = task.CareScheduleId,
            CareScheduleTitle = careScheduleTitle,
            CreatedBy = task.CreatedBy,
            CreatedByName = createdByName,
            AssignedTo = task.AssignedTo,
            AssignedToName = assignedToName,
            SkillRequirements = skillReqDtos,
            Assignments = assignmentDtos
        };
    }

    private static TaskAssignmentResponseDto MapAssignmentToResponseDto(SmartFarmSEP490.Model.TaskAssignment assignment)
    {
        var roleName = assignment.Assignee?.UserRoles?.FirstOrDefault()?.Role?.RoleName ?? "";

        var skills = assignment.Assignee?.UserSkills?
            .Select(us => new AssigneeSkillDto
            {
                SkillId = us.SkillId,
                SkillName = us.Skill?.SkillName ?? "",
                ProficiencyLevel = us.ProficiencyLevel
            }).ToList() ?? new List<AssigneeSkillDto>();

        return new TaskAssignmentResponseDto
        {
            Id = assignment.Id,
            TaskId = assignment.TaskId,
            TaskTitle = assignment.Task?.Title ?? "",
            AssigneeId = assignment.AssigneeId,
            AssigneeName = assignment.Assignee?.FullName ?? "",
            AssigneeEmail = assignment.Assignee?.Email ?? "",
            AssigneeRole = roleName,
            AssigneeSkills = skills,
            AssignedBy = assignment.AssignedBy,
            AssignedByName = assignment.AssignedByNavigation?.FullName,
            Reason = assignment.Reason,
            Status = assignment.Status.ToString(),
            AssignedAt = assignment.AssignedAt,
            EndedAt = assignment.EndedAt
        };
    }
}
