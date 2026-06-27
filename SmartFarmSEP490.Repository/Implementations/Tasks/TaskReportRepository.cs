using Microsoft.EntityFrameworkCore;
using SmartFarmSEP490.Model;
using SmartFarmSEP490.Repository.DbContexts;
using SmartFarmSEP490.Repository.Interfaces.Tasks;
using Task = System.Threading.Tasks.Task;

namespace SmartFarmSEP490.Repository.Implementations.Tasks;

public class TaskReportRepository : ITaskReportRepository
{
    private readonly SmartFarmDbContext _context;
    public TaskReportRepository(SmartFarmDbContext context) => _context = context;

    public async Task<TaskReport?> GetByIdAsync(Guid id) =>
        await _context.TaskReports
            .Include(r => r.Reporter)
            .Include(r => r.Task)
            .FirstOrDefaultAsync(r => r.Id == id);

    public async Task<List<TaskReport>> GetByTaskIdAsync(Guid taskId) =>
        await _context.TaskReports
            .Include(r => r.Reporter)
            .Include(r => r.PlantImages)
            .Where(r => r.TaskId == taskId)
            .OrderByDescending(r => r.ReportedAt)
            .ToListAsync();

    public async Task<List<TaskReport>> GetByBatchIdAsync(Guid batchId) =>
        await _context.TaskReports
            .Include(r => r.Reporter)
            .Include(r => r.Task)
                .ThenInclude(t => t.Batch)
            .Include(r => r.PlantImages)
            .Where(r => r.Task.BatchId == batchId)
            .OrderByDescending(r => r.ReportedAt)
            .ToListAsync();

    public async Task<TaskReport> CreateAsync(TaskReport entity)
    {
        entity.ReportedAt = DateTime.UtcNow;
        await _context.TaskReports.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(TaskReport entity)
    {
        _context.TaskReports.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var e = await _context.TaskReports.FindAsync(id);
        if (e != null) { _context.TaskReports.Remove(e); await _context.SaveChangesAsync(); }
    }
}
