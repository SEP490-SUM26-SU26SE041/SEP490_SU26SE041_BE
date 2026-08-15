using Microsoft.EntityFrameworkCore;
using SmartFarmSEP490.Model;
using SmartFarmSEP490.Model.DTOs;
using SmartFarmSEP490.Model.Helpers;
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
        // Cửa sổ "hôm nay" theo ICT giờ làm việc: [00:00 → 17:00 ICT].
        // DueDate được lưu UTC; convert 17:00 ICT → 10:00 UTC cùng ngày.
        var (startUtc, endUtc) = VietnamTime.GetVietnamWorkdayWindowUtc();
        return await FullQuery()
            .Where(t => t.AssignedTo == assigneeId
                && t.DueDate.HasValue
                && t.DueDate.Value >= startUtc
                && t.DueDate.Value < endUtc)
            .OrderBy(t => t.DueDate)
            .ToListAsync();
    }

    public async Task<List<Model.Task>> GetUpcomingTasksAsync(Guid assigneeId, int days)
    {
        var (todayIctStart, _) = VietnamTime.GetVietnamDayWindowUtc();
        var future = todayIctStart.AddDays(days);
        return await FullQuery()
            .Where(t => t.AssignedTo == assigneeId
                && t.DueDate.HasValue
                && t.DueDate.Value >= todayIctStart
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
        var (todayIctStart, tomorrowIctStart) = VietnamTime.GetVietnamDayWindowUtc(now);
        var upcomingDays = filter.UpcomingDays ?? 7;
        var future = todayIctStart.AddDays(upcomingDays);

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
                // Cửa sổ "hôm nay" theo ICT giờ làm việc: [00:00 → 17:00 ICT] = [00:00 → 10:00 UTC cùng ngày ICT].
                // Dùng helper chung để tránh lệch logic giữa các query.
                var (todayWorkStart, todayWorkEnd) = VietnamTime.GetVietnamWorkdayWindowUtc(now);
                query = query.Where(t => t.DueDate.HasValue
                    && t.DueDate.Value >= todayWorkStart
                    && t.DueDate.Value < todayWorkEnd);
                query = query.OrderBy(t => t.DueDate);
                break;

            case TaskFilterScope.Upcoming:
                // "Sắp tới" tính từ 00:00 ICT ngày mai.
                query = query.Where(t => t.DueDate.HasValue
                    && t.DueDate.Value >= tomorrowIctStart
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

    public async Task<List<Model.Task>> GetActiveTasksDueOnDayAsync(
        DateTime dayStartUtc,
        DateTime dayEndUtc,
        CancellationToken ct = default)
    {
        return await _context.Tasks
            .AsNoTracking()
            .Where(t => t.DueDate != null
                     && t.DueDate >= dayStartUtc
                     && t.DueDate < dayEndUtc
                     && (t.Status == TaskStatus.Pending || t.Status == TaskStatus.InProgress))
            .OrderBy(t => t.DueDate)
            .ToListAsync(ct);
    }

    public async Task<List<TaskCountByUserRow>> CountTasksByUserAsync(
        IReadOnlyCollection<string> roleNames,
        DateTime startUtc,
        DateTime endUtc,
        CancellationToken ct = default)
    {
        if (roleNames == null || roleNames.Count == 0)
            return new List<TaskCountByUserRow>();

        // Single GROUP BY query: AssignedTo + Status trong cùng 1 SQL round-trip.
        // Lưu ý: Postgres enum không cast trực tiếp sang int, nên phải cast qua text trước:
        //   CAST(t."Status" AS text) -> đối chiếu trong CASE -> ép về int.
        // Dùng client-side mapping (PostgresValueGenerationStrategy.None) sẽ gây lỗi nếu dùng
        // EF Translate; ta nhóm theo status string rồi map lại int trong bộ nhớ.
        var grouped = await _context.Tasks
            .AsNoTracking()
            .Where(t => t.AssignedTo != null
                     && t.DueDate != null
                     && t.DueDate >= startUtc
                     && t.DueDate < endUtc
                     && _context.UserRoles
                         .Where(ur => ur.UserId == t.AssignedTo
                                   && roleNames.Contains(ur.Role.RoleName))
                         .Any())
            .GroupBy(t => new { t.AssignedTo, t.Status })
            .Select(g => new
            {
                UserId = g.Key.AssignedTo!.Value,
                Status = g.Key.Status,
                Count = g.Count()
            })
            .ToListAsync(ct);

        return grouped
            .Select(g => new TaskCountByUserRow
            {
                UserId = g.UserId,
                Status = (int)g.Status,
                Count = g.Count
            })
            .ToList();
    }
}
