using SmartFarmSEP490.Model;
using SmartFarmSEP490.Model.DTOs;
using SmartFarmSEP490.Model.Enums;
using SmartFarmSEP490.Repository.DbContexts;
using SmartFarmSEP490.Repository.Interfaces.Auth;
using SmartFarmSEP490.Repository.Interfaces.Batches;
using SmartFarmSEP490.Repository.Interfaces.CareSchedules;
using SmartFarmSEP490.Repository.Interfaces.ExperimentStages;
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
    private readonly IExperimentStageRepository _stageRepository;
    private readonly ICareScheduleRepository _careScheduleRepository;
    private readonly IBatchRepository _batchRepository;
    private readonly IUserRepository _userRepository;
    private readonly SmartFarmDbContext _context;

    private static readonly string[] AssignableRoles = { "Technician", "Student" };

    public TaskService(
        ITaskRepository taskRepository,
        ITaskAssignmentRepository assignmentRepository,
        ITaskSkillRequirementRepository skillRequirementRepository,
        IExperimentRepository experimentRepository,
        IExperimentStageRepository stageRepository,
        ICareScheduleRepository careScheduleRepository,
        IBatchRepository batchRepository,
        IUserRepository userRepository,
        SmartFarmDbContext context)
    {
        _taskRepository = taskRepository;
        _assignmentRepository = assignmentRepository;
        _skillRequirementRepository = skillRequirementRepository;
        _experimentRepository = experimentRepository;
        _stageRepository = stageRepository;
        _careScheduleRepository = careScheduleRepository;
        _batchRepository = batchRepository;
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
        if (experiment == null) return null;

        var task = new Model.Task
        {
            Id = Guid.NewGuid(),
            ExperimentId = dto.ExperimentId,
            ExperimentStageId = dto.ExperimentStageId,
            BatchId = dto.BatchId,
            CareScheduleId = dto.CareScheduleId,
            CreatedBy = createdById,
            Type = Enum.TryParse<Model.Enums.TaskType>(dto.TaskType, ignoreCase: true, out var tt) ? tt : Model.Enums.TaskType.Other,
            Title = dto.Title,
            Description = dto.Description,
            RequiredSkillDescription = dto.RequiredSkillDescription,
            DueDate = dto.DueDate,
            Status = Model.Enums.TaskStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _taskRepository.AddAsync(task);
        return await MapToResponseDto(task);
    }

    public async System.Threading.Tasks.Task<TaskResponseDto?> GetByIdAsync(Guid id)
    {
        var task = await _taskRepository.GetByIdAsync(id);
        return task == null ? null : await MapToResponseDto(task);
    }

    public async System.Threading.Tasks.Task<List<TaskResponseDto>> GetByExperimentAsync(Guid experimentId)
    {
        var tasks = await _taskRepository.GetByExperimentAsync(experimentId);
        var results = new List<TaskResponseDto>();
        foreach (var t in tasks) results.Add(await MapToResponseDto(t));
        return results;
    }

    public async System.Threading.Tasks.Task<List<TaskResponseDto>> GetByStageAsync(Guid stageId)
    {
        var tasks = await _taskRepository.GetByStageAsync(stageId);
        var results = new List<TaskResponseDto>();
        foreach (var t in tasks) results.Add(await MapToResponseDto(t));
        return results;
    }

    public async System.Threading.Tasks.Task<List<TaskResponseDto>> GetByBatchAsync(Guid batchId)
    {
        var tasks = await _taskRepository.GetByBatchAsync(batchId);
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

    public async System.Threading.Tasks.Task<List<TaskResponseDto>> GetTodayTasksAsync(Guid assigneeId)
    {
        var tasks = await _taskRepository.GetTodayTasksAsync(assigneeId);
        var results = new List<TaskResponseDto>();
        foreach (var t in tasks) results.Add(await MapToResponseDto(t));
        return results;
    }

    public async System.Threading.Tasks.Task<List<TaskResponseDto>> GetUpcomingTasksAsync(Guid assigneeId, int days)
    {
        var tasks = await _taskRepository.GetUpcomingTasksAsync(assigneeId, days);
        var results = new List<TaskResponseDto>();
        foreach (var t in tasks) results.Add(await MapToResponseDto(t));
        return results;
    }

    public async System.Threading.Tasks.Task<List<TaskResponseDto>> GetOverdueTasksAsync(Guid assigneeId)
    {
        var tasks = await _taskRepository.GetOverdueTasksAsync(assigneeId);
        var results = new List<TaskResponseDto>();
        foreach (var t in tasks) results.Add(await MapToResponseDto(t));
        return results;
    }

    public async System.Threading.Tasks.Task<List<TaskResponseDto>> GetResearcherCreatedTasksAsync(ResearcherCreatedTaskFilterDto filter)
    {
        if (filter == null || !filter.CreatorId.HasValue)
            return new List<TaskResponseDto>();

        var tasks = await _taskRepository.GetResearcherCreatedTasksAsync(filter);
        var results = new List<TaskResponseDto>();
        foreach (var t in tasks) results.Add(await MapToResponseDto(t));
        return results;
    }

    public async System.Threading.Tasks.Task<TaskResponseDto?> UpdateAsync(Guid id, UpdateTaskDto dto, Guid userId)
    {
        var task = await _taskRepository.GetByIdAsync(id);
        if (task == null) return null;

        if (dto.ExperimentStageId != null) task.ExperimentStageId = dto.ExperimentStageId;
        if (dto.BatchId != null) task.BatchId = dto.BatchId;
        if (dto.CareScheduleId != null) task.CareScheduleId = dto.CareScheduleId;
        if (!string.IsNullOrEmpty(dto.TaskType)) task.Type = Enum.TryParse<Model.Enums.TaskType>(dto.TaskType, ignoreCase: true, out var tt) ? tt : task.Type;
        if (!string.IsNullOrEmpty(dto.Title)) task.Title = dto.Title;
        if (dto.Description != null) task.Description = dto.Description;
        if (dto.RequiredSkillDescription != null) task.RequiredSkillDescription = dto.RequiredSkillDescription;
        if (dto.DueDate.HasValue) task.DueDate = dto.DueDate;
        if (!string.IsNullOrEmpty(dto.Status)) task.Status = Enum.TryParse<Model.Enums.TaskStatus>(dto.Status, ignoreCase: true, out var ts) ? ts : task.Status;
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

    public async System.Threading.Tasks.Task<GenerateByStageResultDto> GenerateByStageAsync(Guid stageId, Guid userId)
    {
        var stage = await _stageRepository.GetByIdAsync(stageId);
        if (stage == null)
        {
            return new GenerateByStageResultDto
            {
                StageId = stageId,
                Tasks = new(),
                HasError = true,
                Message = $"Stage with id '{stageId}' was not found."
            };
        }

        var schedules = await _careScheduleRepository.GetByStageAsync(stageId);

        // CHECK: Stage phải có ít nhất 1 CareSchedule trước khi sinh task
        if (schedules == null || schedules.Count == 0)
        {
            return new GenerateByStageResultDto
            {
                StageId = stageId,
                StageName = stage.StageName,
                StageStartDate = stage.StartDate,
                StageEndDate = stage.EndDate,
                TotalSchedules = 0,
                TasksGenerated = 0,
                TasksSkipped = 0,
                HasError = true,
                Message = $"Stage '{stage.StageName}' chưa có CareSchedule nào. Vui lòng tạo CareSchedule cho giai đoạn này trước khi sinh task.",
                Tasks = new()
            };
        }

        // CHECK: Stage đã được generate task trước đó chưa?
        var existingStageTasks = await _taskRepository.GetByStageAsync(stageId);
        if (existingStageTasks != null && existingStageTasks.Count > 0)
        {
            return new GenerateByStageResultDto
            {
                StageId = stageId,
                StageName = stage.StageName,
                StageStartDate = stage.StartDate,
                StageEndDate = stage.EndDate,
                TotalSchedules = schedules.Count,
                TasksGenerated = 0,
                TasksSkipped = existingStageTasks.Count,
                ExistingTasksCount = existingStageTasks.Count,
                HasError = true,
                Message = $"Stage '{stage.StageName}' đã được tạo task rồi ({existingStageTasks.Count} task hiện có). Vui lòng xóa các task đã phát sinh của stage này trước khi sinh lại.",
                Tasks = new()
            };
        }

        var result = new GenerateByStageResultDto
        {
            StageId = stageId,
            StageName = stage.StageName,
            StageStartDate = stage.StartDate,
            StageEndDate = stage.EndDate,
            TotalSchedules = schedules.Count,
            Tasks = new List<GeneratedTaskResultDto>()
        };

        foreach (var schedule in schedules)
        {
            // Determine which batches this schedule applies to
            var targetBatchIds = new List<Guid>();
            if (schedule.BatchId.HasValue)
            {
                targetBatchIds.Add(schedule.BatchId.Value);
            }
            else
            {
                // Schedule không gán batch cụ thể -> apply cho tất cả batches của experiment
                var allBatches = await _batchRepository.GetByExperimentAsync(schedule.ExperimentId);
                foreach (var b in allBatches) targetBatchIds.Add(b.Id);
            }

            // Determine time window: prefer Stage date range, fallback to schedule
            var startDate = stage.StartDate ?? schedule.StartDate;
            var endDate = stage.EndDate ?? schedule.EndDate ?? DateOnly.FromDateTime(DateTime.UtcNow);

            if (startDate > endDate)
            {
                result.TasksSkipped++;
                continue;
            }

            // Compute occurrences
            var frequencyDays = schedule.FrequencyDays ?? 0;
            List<DateOnly> dueDates;

            if (frequencyDays > 0)
            {
                int totalDays = endDate.DayNumber - startDate.DayNumber;
                int occurrences = Math.Max(1, (int)Math.Ceiling((double)(totalDays + 1) / frequencyDays));
                dueDates = new List<DateOnly>();
                for (int i = 0; i < occurrences; i++)
                {
                    var due = startDate.AddDays(i * frequencyDays);
                    if (due > endDate) break;
                    dueDates.Add(due);
                }
            }
            else
            {
                dueDates = new List<DateOnly> { startDate };
            }

            foreach (var batchId in targetBatchIds)
            {
                foreach (var dueDate in dueDates)
                {
                    var dueDateTime = dueDate.ToDateTime(TimeOnly.MinValue);

                    var existing = await _taskRepository.GetByBatchAsync(batchId);
                    var alreadyExists = existing.Any(t =>
                        t.CareScheduleId == schedule.Id &&
                        t.Status != Model.Enums.TaskStatus.Completed &&
                        t.DueDate.HasValue &&
                        t.DueDate.Value.Date == dueDateTime.Date);

                    if (alreadyExists)
                    {
                        result.TasksSkipped++;
                        continue;
                    }

                    var task = new Model.Task
                    {
                        Id = Guid.NewGuid(),
                        ExperimentId = schedule.ExperimentId,
                        ExperimentStageId = schedule.ExperimentStageId,
                        BatchId = batchId,
                        CareScheduleId = schedule.Id,
                        CreatedBy = userId,
                        Type = schedule.TaskType,
                        Title = schedule.Title,
                        Description = schedule.Instruction,
                        DueDate = dueDateTime,
                        Status = Model.Enums.TaskStatus.Pending,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    await _taskRepository.AddAsync(task);
                    result.TasksGenerated++;

                    result.Tasks.Add(new GeneratedTaskResultDto
                    {
                        TaskId = task.Id,
                        Title = task.Title,
                        TaskType = task.Type.ToString(),
                        Status = task.Status.ToString(),
                        DueDate = task.DueDate,
                        BatchId = batchId,
                        ScheduleId = schedule.Id,
                        IsNew = true,
                        Message = $"Task scheduled for {dueDate:yyyy-MM-dd}"
                    });
                }
            }
        }

        return result;
    }

    public async System.Threading.Tasks.Task<GenerateByExperimentResultDto> GenerateByExperimentAsync(Guid experimentId, Guid userId)
    {
        var stages = await _stageRepository.GetByExperimentAsync(experimentId);
        var result = new GenerateByExperimentResultDto
        {
            ExperimentId = experimentId,
            TotalStages = stages.Count,
            StageResults = new List<GenerateByStageResultDto>()
        };

        foreach (var stage in stages.OrderBy(s => s.StageOrder))
        {
            // Delegate xử lý từng stage cho GenerateByStageAsync để đảm bảo logic đồng nhất:
            //   - Check CareSchedule tồn tại
            //   - Check stage đã generate task chưa
            //   - Tính FrequencyDays, due dates
            //   - Xử lý BatchId == null (apply cho tất cả batches)
            //   - Dedupe theo CareScheduleId + DueDate
            var stageResult = await GenerateByStageAsync(stage.Id, userId);
            result.StageResults.Add(stageResult);
            result.TotalSchedules += stageResult.TotalSchedules;

            if (stageResult.HasError)
            {
                // Stage bị skip vì lý do nghiệp vụ (chưa có CareSchedule / đã có task)
                result.StagesSkipped++;
                continue;
            }
            result.TasksGenerated += stageResult.TasksGenerated;
            result.TasksSkipped += stageResult.TasksSkipped;
            result.Tasks.AddRange(stageResult.Tasks);
        }
                // Thông báo tổng hợp nếu toàn bộ experiment đều bị skip
                if (result.TasksGenerated == 0 && result.StagesSkipped == result.TotalStages && result.TotalStages > 0)
                {
                    result.HasError = true;
                    result.Message = $"Không thể sinh task cho experiment. Tất cả {result.TotalStages} stage đều chưa có CareSchedule hoặc đã được generate task trước đó.";
                }

                return result;
    }

    public async System.Threading.Tasks.Task<TaskResponseDto?> UpdateTaskStatusAsync(Guid id, string status, Guid userId)
    {
        var task = await _taskRepository.GetByIdAsync(id);
        if (task == null) return null;

        task.Status = Enum.TryParse<Model.Enums.TaskStatus>(status, ignoreCase: true, out var ts) ? ts : task.Status;
        task.UpdatedAt = DateTime.UtcNow;
        await _taskRepository.UpdateAsync(task);

        return await MapToResponseDto(task);
    }

    public async System.Threading.Tasks.Task<TaskResponseDto?> AssignTaskAsync(AssignTaskDto dto, Guid assignedById)
    {
        var task = await _taskRepository.GetByIdAsync(dto.TaskId);
        if (task == null) return null;

        var assignee = await _userRepository.GetUserByIdAsync(dto.AssigneeId);
        if (assignee == null) return null;

        var isAllowedRole = assignee.UserRoles.Any(ur => AssignableRoles.Contains(ur.Role.RoleName));
        if (!isAllowedRole) return null;

        var existingActive = await _assignmentRepository.GetActiveByTaskAndAssigneeAsync(dto.TaskId, dto.AssigneeId);
        if (existingActive != null) return null;

        var assignment = new Model.TaskAssignment
        {
            Id = Guid.NewGuid(),
            TaskId = dto.TaskId,
            AssigneeId = dto.AssigneeId,
            AssignedBy = assignedById,
            Status = Model.Enums.TaskAssignmentStatus.Assigned,
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
        if (task == null) return null;

        var newAssignee = await _userRepository.GetUserByIdAsync(dto.NewAssigneeId);
        if (newAssignee == null) return null;

        var isAllowedRole = newAssignee.UserRoles.Any(ur => AssignableRoles.Contains(ur.Role.RoleName));
        if (!isAllowedRole) return null;

        var existingAssignments = await _assignmentRepository.GetByTaskIdAsync(dto.TaskId);
        foreach (var existing in existingAssignments.Where(a => a.EndedAt == null))
        {
            existing.Status = Model.Enums.TaskAssignmentStatus.Reassigned;
            existing.EndedAt = DateTime.UtcNow;
            await _assignmentRepository.UpdateAsync(existing);
        }

        var newAssignment = new Model.TaskAssignment
        {
            Id = Guid.NewGuid(),
            TaskId = dto.TaskId,
            AssigneeId = dto.NewAssigneeId,
            AssignedBy = reassignedById,
            Status = Model.Enums.TaskAssignmentStatus.Assigned,
            Reason = dto.Reason,
            AssignedAt = DateTime.UtcNow
        };

        await _assignmentRepository.AddAsync(newAssignment);

        task.AssignedTo = dto.NewAssigneeId;
        task.UpdatedAt = DateTime.UtcNow;
        await _taskRepository.UpdateAsync(task);

        return await MapToResponseDto(task);
    }

    public async System.Threading.Tasks.Task<TaskAssignmentResponseDto?> UpdateAssignmentStatusAsync(UpdateTaskAssignmentStatusDto dto)
    {
        var assignment = await _assignmentRepository.GetByIdAsync(dto.AssignmentId);
        if (assignment == null) return null;

        assignment.Status = Enum.TryParse<Model.Enums.TaskAssignmentStatus>(dto.Status, ignoreCase: true, out var as_) ? as_ : assignment.Status;
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
                task.Status = Model.Enums.TaskStatus.Completed;
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

    private async System.Threading.Tasks.Task<TaskResponseDto> MapToResponseDto(Model.Task task)
    {
        var experiment = await _experimentRepository.GetByIdAsync(task.ExperimentId);

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
            ExperimentTitle = experiment?.Title,
            ExperimentCode = experiment?.ExperimentCode,
            ExperimentStageId = task.ExperimentStageId,
            ExperimentStageName = task.ExperimentStage?.StageName,
            BatchId = task.BatchId,
            BatchCode = task.Batch?.BatchCode,
            CareScheduleId = task.CareScheduleId,
            CareScheduleTitle = task.CareSchedule?.Title,
            CreatedBy = task.CreatedBy,
            CreatedByName = task.CreatedByNavigation?.FullName,
            AssignedTo = task.AssignedTo,
            AssignedToName = task.AssignedToNavigation?.FullName,
            SkillRequirements = skillReqDtos,
            Assignments = assignmentDtos
        };
    }

    private static TaskAssignmentResponseDto MapAssignmentToResponseDto(Model.TaskAssignment assignment)
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
