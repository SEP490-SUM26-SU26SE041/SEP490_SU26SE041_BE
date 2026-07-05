using Microsoft.EntityFrameworkCore;
using SmartFarmSEP490.Model;
using SmartFarmSEP490.Repository.DbContexts;
using SmartFarmSEP490.Repository.Interfaces.Tasks;
using Task = System.Threading.Tasks.Task;

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
}
