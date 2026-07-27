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
        return await Task.FromResult(_context.ExperimentReports.ToList());
    }

    public async Task<List<ExperimentReport>> GetByExperimentAsync(Guid experimentId)
    {
        return await Task.FromResult(
            _context.ExperimentReports
                .Where(r => r.ExperimentId == experimentId)
                .OrderByDescending(r => r.CreatedAt)
                .ToList());
    }

    public async Task<ExperimentReport?> GetByIdAsync(Guid id)
    {
        return await Task.FromResult(
            _context.ExperimentReports.FirstOrDefault(r => r.Id == id));
    }

    public async Task<ExperimentReport> AddAsync(ExperimentReport report)
    {
        report.CreatedAt = DateTime.UtcNow;
        _context.ExperimentReports.Add(report);
        await Task.CompletedTask;
        return report;
    }

    public async Task UpdateAsync(ExperimentReport report)
    {
        _context.ExperimentReports.Update(report);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id)
    {
        var report = await GetByIdAsync(id);
        if (report != null)
        {
            _context.ExperimentReports.Remove(report);
        }
        await Task.CompletedTask;
    }
}
