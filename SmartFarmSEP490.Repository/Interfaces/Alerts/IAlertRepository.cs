using SmartFarmSEP490.Model;
using SmartFarmSEP490.Model.Enums;
using Task = System.Threading.Tasks.Task;

namespace SmartFarmSEP490.Repository.Interfaces.Alerts;

public interface IAlertRepository
{
    Task<List<Alert>> GetAllAsync();
    Task<List<Alert>> GetByExperimentAsync(Guid experimentId);
    Task<List<Alert>> GetBySensorAsync(Guid sensorId);
    Task<List<Alert>> GetActiveAlertsAsync();
    Task<List<Alert>> GetActiveAlertsBySeverityAsync(AlertSeverity severity);
    Task<List<Alert>> GetActiveAlertsByExperimentAsync(Guid experimentId);
    Task<Alert?> GetByIdAsync(Guid id);
    Task<int> GetActiveAlertCountAsync();
    Task<int> GetCriticalAlertCountAsync();
    Task<Alert> AddAsync(Alert alert);
    Task UpdateAsync(Alert alert);
    Task DeleteAsync(Guid id);
}
