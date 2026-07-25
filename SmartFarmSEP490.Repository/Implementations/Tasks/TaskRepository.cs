using Microsoft.EntityFrameworkCore;
using SmartFarmSEP490.Model;
using SmartFarmSEP490.Model.DTOs;
using SmartFarmSEP490.Repository.DbContexts;
using SmartFarmSEP490.Repository.Interfaces.Tasks;
using Task = System.Threading.Tasks.Task;
using TaskStatus = SmartFarmSEP490.Model.Enums.TaskStatus;

namespace SmartFarmSEP490.Repository.Implementations.Tasks;

public class TaskRepository : ITaskRepository
{
    private readonly SmartFarmDbContext _context;
    public TaskRepository(SmartFarmDbContext context) => _context = context;

    private IQueryable<Model.Task> FullQuery() =>
        _context.Tasks
            .Include(t => t.Experiment)
            .Include(t => t.ExperimentStage)
            .Include(t => t.Batch)
            .Include(t => t.CareSchedule)
            .Include(t => t.CreatedByNavigation)
            .Include(t => t.AssignedToNavigation)
            .Include(t => t.TaskSkillRequirements)
                .ThenInclude(tsr => tsr.Skill)
            .Include(t => t.TaskAssignments)
                .ThenInclude(ta => ta.Assignee)
                    .ThenInclude(a => a.UserRoles)
                        .ThenInclude(ur => ur.Role)
            .Include(t => t.TaskAssignments)
                .ThenInclude(ta => ta.AssignedByNavigation)
            .Include(t => t.TaskReports)
                .ThenInclude(tr => tr.Reporter)
            .Include(t => t.TaskReports)
                .ThenInclude(tr => tr.PlantImages);

    public async Task<Model.Task?> GetByIdAsync(Guid id) =>
        await FullQuery().FirstOrDefaultAsync(t => t.Id == id);

    public async Task<List<Model.Task>> GetByExperimentAsync(Guid experimentId) =>
        await FullQuery()
            .Where(t => t.ExperimentId == experimentId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

    public async Task<List<Model.Task>> GetByStageAsync(Guid stageId) =>
        await FullQuery()
            .Where(t => t.ExperimentStageId == stageId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

    public async Task<List<Model.Task>> GetByBatchAsync(Guid batchId) =>
        await FullQuery()
            .Where(t => t.BatchId == batchId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

    public async Task<List<Model.Task>> GetByAssigneeAsync(Guid assigneeId) =>
        await FullQuery()
            .Where(t => t.AssignedTo == assigneeId)
            .OrderByDescending(t => t.DueDate)
            .ToListAsync();

    public async Task<List<Model.Task>> GetMyTasksAsync(Guid assigneeId, MyTaskFilterDto filter, CancellationToken ct = default)
    {
        // Chuẩn hóa filter (lowercase, trim, distinct, loại rỗng)
        var rawStatuses = filter?.Statuses ?? new List<string>();
        var statusEnums = new List<TaskStatus>();
        foreach (var raw in rawStatuses)
        {
            if (string.IsNullOrWhiteSpace(raw)) continue;
            if (Enum.TryParse<TaskStatus>(raw.Trim(), ignoreCase: true, out var parsed))
                statusEnums.Add(parsed);
        }
        var distinctStatuses = statusEnums.Distinct().ToList();

        var query = FullQuery().Where(t => t.AssignedTo == assigneeId);

        if (filter != null)
        {
            if (filter.BatchId.HasValue)
                query = query.Where(t => t.BatchId == filter.BatchId.Value);

            if (filter.ExperimentId.HasValue)
                query = query.Where(t => t.ExperimentId == filter.ExperimentId.Value);

            if (distinctStatuses.Count > 0)
                query = query.Where(t => distinctStatuses.Contains(t.Status));
        }

        return await query
            .OrderByDescending(t => t.DueDate)
            .ToListAsync(ct);
    }

    public async Task<List<Model.Task>> GetAllAsync() =>
        await FullQuery()
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

    public async Task<List<Model.Task>> GetTodayTasksAsync(Guid assigneeId)
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);
        return await FullQuery()
            .Where(t => t.AssignedTo == assigneeId
                && t.DueDate.HasValue
                && t.DueDate.Value >= today
                && t.DueDate.Value < tomorrow)
            .OrderBy(t => t.DueDate)
            .ToListAsync();
    }

    public async Task<List<Model.Task>> GetUpcomingTasksAsync(Guid assigneeId, int days)
    {
        var today = DateTime.UtcNow.Date;
        var future = today.AddDays(days);
        return await FullQuery()
            .Where(t => t.AssignedTo == assigneeId
                && t.DueDate.HasValue
                && t.DueDate.Value >= today
                && t.DueDate.Value <= future)
            .OrderBy(t => t.DueDate)
            .ToListAsync();
    }

    public async Task<List<Model.Task>> GetOverdueTasksAsync(Guid assigneeId)
    {
        var now = DateTime.UtcNow;
        var completedStatus = SmartFarmSEP490.Model.Enums.TaskStatus.Completed;
        return await FullQuery()
            .Where(t => t.AssignedTo == assigneeId
                && t.DueDate.HasValue
                && t.DueDate.Value < now
                && t.Status != completedStatus)
            .OrderBy(t => t.DueDate)
            .ToListAsync();
    }

    public async Task<List<Model.Task>> GetResearcherCreatedTasksAsync(ResearcherCreatedTaskFilterDto filter)
    {
        var completedStatus = SmartFarmSEP490.Model.Enums.TaskStatus.Completed;
        var cancelledStatus = SmartFarmSEP490.Model.Enums.TaskStatus.Cancelled;
        var now = DateTime.UtcNow;
        var today = now.Date;
        var tomorrow = today.AddDays(1);
        var upcomingDays = filter.UpcomingDays ?? 7;
        var future = today.AddDays(upcomingDays);

        var query = FullQuery().Where(t => t.CreatedBy == filter.CreatorId);

        if (filter.ExperimentId.HasValue)
            query = query.Where(t => t.ExperimentId == filter.ExperimentId.Value);

        var scope = (filter.Scope ?? string.Empty).Trim().ToLowerInvariant();
        switch (scope)
        {
            case TaskFilterScope.Overdue:
                query = query.Where(t => t.DueDate.HasValue
                    && t.DueDate.Value < now
                    && t.Status != completedStatus
                    && t.Status != cancelledStatus);
                query = query.OrderBy(t => t.DueDate);
                break;

            case TaskFilterScope.Today:
                query = query.Where(t => t.DueDate.HasValue
                    && t.DueDate.Value >= today
                    && t.DueDate.Value < tomorrow);
                query = query.OrderBy(t => t.DueDate);
                break;

            case TaskFilterScope.Upcoming:
                query = query.Where(t => t.DueDate.HasValue
                    && t.DueDate.Value >= tomorrow
                    && t.DueDate.Value <= future);
                query = query.OrderBy(t => t.DueDate);
                break;

            default:
                query = query.OrderByDescending(t => t.CreatedAt);
                break;
        }

        return await query.ToListAsync();
    }

    public async Task<Model.Task> AddAsync(Model.Task task)
    {
        await _context.Tasks.AddAsync(task);
        await _context.SaveChangesAsync();
        return task;
    }

    public async Task UpdateAsync(Model.Task task)
    {
        _context.Tasks.Update(task);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var task = await _context.Tasks.FindAsync(id);
        if (task != null)
        {
            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<Model.Task>> GetOverdueCandidatesAsync(DateTime asOfUtc, CancellationToken ct = default)
    {
        return await _context.Tasks
            .AsNoTracking()
            .Where(t => t.DueDate != null
                     && t.DueDate < asOfUtc
                     && (t.Status == TaskStatus.Pending || t.Status == TaskStatus.InProgress))
            .OrderBy(t => t.DueDate)
            .ToListAsync(ct);
    }

    public async Task<int> MarkOverdueAsync(DateTime asOfUtc, CancellationToken ct = default)
    {
        // Single SQL UPDATE — idempotent vì WHERE Status IN (Pending, InProgress)
        return await _context.Tasks
            .Where(t => t.DueDate != null
                     && t.DueDate < asOfUtc
                     && (t.Status == TaskStatus.Pending || t.Status == TaskStatus.InProgress))
            .ExecuteUpdateAsync(s => s
                .SetProperty(t => t.Status, TaskStatus.Overdue)
                .SetProperty(t => t.UpdatedAt, DateTime.UtcNow), ct);
    }
}
