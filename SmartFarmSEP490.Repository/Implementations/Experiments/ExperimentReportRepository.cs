using Microsoft.EntityFrameworkCore;
using SmartFarmSEP490.Model;
using SmartFarmSEP490.Repository.DbContexts;
using SmartFarmSEP490.Repository.Interfaces.Experiments;
using Task = System.Threading.Tasks.Task;

namespace SmartFarmSEP490.Repository.Implementations.Experiments;

public class ExperimentReportRepository : IExperimentReportRepository
{
    private readonly SmartFarmDbContext _context;

    public ExperimentReportRepository(SmartFarmDbContext context)
    {
        _context = context;
    }

    public async Task<List<ExperimentReport>> GetAllAsync()
    {
        return await _context.ExperimentReports
            .Include(r => r.Experiment)
            .Include(r => r.CreatedByNavigation)
            .ToListAsync();
    }

    public async Task<List<ExperimentReport>> GetByExperimentAsync(Guid experimentId)
    {
        return await _context.ExperimentReports
            .Include(r => r.Experiment)
            .Include(r => r.CreatedByNavigation)
            .Where(r => r.ExperimentId == experimentId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<ExperimentReport?> GetByIdAsync(Guid id)
    {
        return await _context.ExperimentReports
            .Include(r => r.Experiment)
            .Include(r => r.CreatedByNavigation)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<ExperimentReport> AddAsync(ExperimentReport report)
    {
        report.CreatedAt = DateTime.UtcNow;
        await _context.ExperimentReports.AddAsync(report);
        await _context.SaveChangesAsync();
        return report;
    }

    public async Task UpdateAsync(ExperimentReport report)
    {
        _context.ExperimentReports.Update(report);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var report = await _context.ExperimentReports.FindAsync(id);
        if (report != null)
        {
            _context.ExperimentReports.Remove(report);
            await _context.SaveChangesAsync();
        }
    }
}
