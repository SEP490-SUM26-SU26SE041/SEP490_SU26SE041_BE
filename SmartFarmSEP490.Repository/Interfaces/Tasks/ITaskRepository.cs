using SmartFarmSEP490.Model;
using SmartFarmSEP490.Model.DTOs;
using Task = System.Threading.Tasks.Task;

namespace SmartFarmSEP490.Repository.Interfaces.Tasks;

public interface ITaskRepository
{
    Task<Model.Task?> GetByIdAsync(Guid id);
    Task<List<Model.Task>> GetByExperimentAsync(Guid experimentId);
    Task<List<Model.Task>> GetByStageAsync(Guid stageId);
    Task<List<Model.Task>> GetByBatchAsync(Guid batchId);
    Task<List<Model.Task>> GetByAssigneeAsync(Guid assigneeId);
    Task<List<Model.Task>> GetAllAsync();
    Task<List<Model.Task>> GetTodayTasksAsync(Guid assigneeId);
    Task<List<Model.Task>> GetUpcomingTasksAsync(Guid assigneeId, int days);
    Task<List<Model.Task>> GetOverdueTasksAsync(Guid assigneeId);
    Task<List<Model.Task>> GetResearcherCreatedTasksAsync(ResearcherCreatedTaskFilterDto filter);
    Task<Model.Task> AddAsync(Model.Task task);
    Task UpdateAsync(Model.Task task);
    Task DeleteAsync(Guid id);

    // My Tasks — filter for the "MY" endpoint (assignee = me + optional status/batch/experiment)
    Task<List<Model.Task>> GetMyTasksAsync(Guid assigneeId, MyTaskFilterDto filter, CancellationToken ct = default);

    // Overdue sweep (UTC-based, idempotent)
    Task<List<Model.Task>> GetOverdueCandidatesAsync(DateTime asOfUtc, CancellationToken ct = default);
    Task<int> MarkOverdueAsync(DateTime asOfUtc, CancellationToken ct = default);

    // Reminder: task chưa hoàn thành có DueDate trong ngày [dayStartUtc, dayEndUtc)
    Task<List<Model.Task>> GetActiveTasksDueOnDayAsync(DateTime dayStartUtc, DateTime dayEndUtc, CancellationToken ct = default);

    // Count task theo user cho Admin report (filter theo role + khoảng thời gian)
    Task<List<TaskCountByUserRow>> CountTasksByUserAsync(
        IReadOnlyCollection<string> roleNames,
        DateTime startUtc,
        DateTime endUtc,
        CancellationToken ct = default);
}

/// <summary>Row thô cho report count task theo user (group by UserId + Status).</summary>
public class TaskCountByUserRow
{
    public Guid UserId { get; set; }
    public int Status { get; set; } // SmartFarmSEP490.Model.Enums.TaskStatus value
    public int Count { get; set; }
}
