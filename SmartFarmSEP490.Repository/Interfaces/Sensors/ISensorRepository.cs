using SmartFarmSEP490.Model;
using Task = System.Threading.Tasks.Task;

namespace SmartFarmSEP490.Repository.Interfaces.Sensors;

public interface ISensorRepository
{
    Task<List<Sensor>> GetAllAsync();
    Task<List<Sensor>> GetByAreaAsync(Guid areaId);
    Task<Sensor?> GetByIdAsync(Guid id);
    Task<List<SensorDatum>> GetSensorDataAsync(Guid sensorId, DateTime? fromDate = null, DateTime? toDate = null, int limit = 100);
    Task<List<SensorDatum>> GetLatestReadingsAsync(Guid? experimentId = null, Guid? batchId = null);
    Task<SensorDatum?> GetLatestReadingBySensorAsync(Guid sensorId);
    Task<List<Sensor>> GetActiveSensorsAsync();
}
