using SmartFarmSEP490.Model.DTOs;

namespace SmartFarmSEP490.Service.Interfaces.Tasks;

public interface ITaskService
{
    // CRUD
    System.Threading.Tasks.Task<TaskResponseDto?> CreateAsync(CreateTaskDto dto, Guid createdById);
    System.Threading.Tasks.Task<TaskResponseDto?> GetByIdAsync(Guid id);
    System.Threading.Tasks.Task<System.Collections.Generic.List<TaskResponseDto>> GetByExperimentAsync(Guid experimentId);
    System.Threading.Tasks.Task<System.Collections.Generic.List<TaskResponseDto>> GetByStageAsync(Guid stageId);
    System.Threading.Tasks.Task<System.Collections.Generic.List<TaskResponseDto>> GetByBatchAsync(Guid batchId);
    System.Threading.Tasks.Task<System.Collections.Generic.List<TaskResponseDto>> GetByAssigneeAsync(Guid assigneeId);
    System.Threading.Tasks.Task<System.Collections.Generic.List<TaskResponseDto>> GetMyTasksAsync(Guid assigneeId, MyTaskFilterDto filter);
    System.Threading.Tasks.Task<System.Collections.Generic.List<TaskResponseDto>> GetAllAsync();
    System.Threading.Tasks.Task<System.Collections.Generic.List<TaskResponseDto>> GetTodayTasksAsync(Guid assigneeId);
    System.Threading.Tasks.Task<System.Collections.Generic.List<TaskResponseDto>> GetUpcomingTasksAsync(Guid assigneeId, int days);
    System.Threading.Tasks.Task<System.Collections.Generic.List<TaskResponseDto>> GetOverdueTasksAsync(Guid assigneeId);
    System.Threading.Tasks.Task<System.Collections.Generic.List<TaskResponseDto>> GetResearcherCreatedTasksAsync(ResearcherCreatedTaskFilterDto filter);
    System.Threading.Tasks.Task<TaskResponseDto?> UpdateAsync(Guid id, UpdateTaskDto dto, Guid userId);
    System.Threading.Tasks.Task<bool> DeleteAsync(Guid id);

    // Task Generation
    System.Threading.Tasks.Task<GenerateByStageResultDto> GenerateByStageAsync(Guid stageId, Guid userId);
    System.Threading.Tasks.Task<GenerateByExperimentResultDto> GenerateByExperimentAsync(Guid experimentId, Guid userId);

    // Task Status
    System.Threading.Tasks.Task<TaskResponseDto?> UpdateTaskStatusAsync(Guid id, string status, Guid userId);

    // Assignment
    System.Threading.Tasks.Task<TaskResponseDto?> AssignTaskAsync(AssignTaskDto dto, Guid assignedById);
    System.Threading.Tasks.Task<TaskResponseDto?> ReassignTaskAsync(ReassignTaskDto dto, Guid reassignedById);
    System.Threading.Tasks.Task<TaskAssignmentResponseDto?> UpdateAssignmentStatusAsync(UpdateTaskAssignmentStatusDto dto);
    System.Threading.Tasks.Task<System.Collections.Generic.List<TaskAssignmentResponseDto>> GetTaskAssignmentsAsync(Guid taskId);
    System.Threading.Tasks.Task<System.Collections.Generic.List<TaskAssignmentResponseDto>> GetAssignmentsByAssigneeAsync(Guid assigneeId);

    // Skill Matching
    System.Threading.Tasks.Task<System.Collections.Generic.List<SkillMatchResultDto>> FindMatchingUsersAsync(Guid taskId);

    // Role validation
    System.Threading.Tasks.Task<bool> ValidateUserRoleAsync(Guid userId, params string[] allowedRoles);
}
