using SmartFarmSEP490.Model;
using SmartFarmSEP490.Model.Enums;
using SmartFarmSEP490.Repository.DbContexts;
using SmartFarmSEP490.Repository.Interfaces.Alerts;
using Task = System.Threading.Tasks.Task;

namespace SmartFarmSEP490.Repository.Implementations.Alerts;

public class AlertRepository : IAlertRepository
{
    private readonly SmartFarmDbContext _context;

    public AlertRepository(SmartFarmDbContext context)
    {
        _context = context;
    }

    public async Task<List<Alert>> GetAllAsync()
    {
        return await Task.FromResult(_context.Alerts.ToList());
    }

    public async Task<List<Alert>> GetByExperimentAsync(Guid experimentId)
    {
        return await Task.FromResult(
            _context.Alerts
                .Where(a => a.ExperimentId == experimentId)
                .ToList());
    }

    public async Task<List<Alert>> GetBySensorAsync(Guid sensorId)
    {
        return await Task.FromResult(
            _context.Alerts
                .Where(a => a.SensorId == sensorId)
                .ToList());
    }

    public async Task<List<Alert>> GetActiveAlertsAsync()
    {
        return await Task.FromResult(
            _context.Alerts
                .Where(a => !a.IsResolved)
                .OrderByDescending(a => a.CreatedAt)
                .ToList());
    }

    public async Task<List<Alert>> GetActiveAlertsBySeverityAsync(AlertSeverity severity)
    {
        return await Task.FromResult(
            _context.Alerts
                .Where(a => !a.IsResolved && a.Severity == severity)
                .OrderByDescending(a => a.CreatedAt)
                .ToList());
    }

    public async Task<List<Alert>> GetActiveAlertsByExperimentAsync(Guid experimentId)
    {
        return await Task.FromResult(
            _context.Alerts
                .Where(a => a.ExperimentId == experimentId && !a.IsResolved)
                .OrderByDescending(a => a.CreatedAt)
                .ToList());
    }

    public async Task<Alert?> GetByIdAsync(Guid id)
    {
        return await Task.FromResult(_context.Alerts.FirstOrDefault(a => a.Id == id));
    }

    public async Task<int> GetActiveAlertCountAsync()
    {
        return await Task.FromResult(_context.Alerts.Count(a => !a.IsResolved));
    }

    public async Task<int> GetCriticalAlertCountAsync()
    {
        return await Task.FromResult(
            _context.Alerts.Count(a => !a.IsResolved && a.Severity == AlertSeverity.Critical));
    }

    public async Task<Alert> AddAsync(Alert alert)
    {
        _context.Alerts.Add(alert);
        await Task.CompletedTask;
        return alert;
    }

    public async Task UpdateAsync(Alert alert)
    {
        _context.Alerts.Update(alert);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id)
    {
        var alert = await GetByIdAsync(id);
        if (alert != null)
        {
            _context.Alerts.Remove(alert);
        }
        await Task.CompletedTask;
    }
}
