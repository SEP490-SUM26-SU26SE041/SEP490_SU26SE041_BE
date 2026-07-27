using SmartFarmSEP490.Model;
using SmartFarmSEP490.Repository.DbContexts;
using SmartFarmSEP490.Repository.Interfaces.Sensors;
using Task = System.Threading.Tasks.Task;

namespace SmartFarmSEP490.Repository.Implementations.Sensors;

public class SensorRepository : ISensorRepository
{
    private readonly SmartFarmDbContext _context;

    public SensorRepository(SmartFarmDbContext context)
    {
        _context = context;
    }

    public async Task<List<Sensor>> GetAllAsync()
    {
        return await Task.FromResult(_context.Sensors.ToList());
    }

    public async Task<List<Sensor>> GetByAreaAsync(Guid areaId)
    {
        var sensorIds = _context.SensorData
            .Where(sd => sd.Batch != null && sd.Batch.ExperimentBedAssignment != null
                && sd.Batch.ExperimentBedAssignment.Bed != null
                && sd.Batch.ExperimentBedAssignment.Bed.AreaId == areaId)
            .Select(sd => sd.SensorId)
            .Distinct()
            .ToList();

        return await Task.FromResult(_context.Sensors.Where(s => sensorIds.Contains(s.Id)).ToList());
    }

    public async Task<Sensor?> GetByIdAsync(Guid id)
    {
        return await Task.FromResult(_context.Sensors.FirstOrDefault(s => s.Id == id));
    }

    public async Task<List<SensorDatum>> GetSensorDataAsync(Guid sensorId, DateTime? fromDate = null, DateTime? toDate = null, int limit = 100)
    {
        var query = _context.SensorData
            .Where(sd => sd.SensorId == sensorId);

        if (fromDate.HasValue)
            query = query.Where(sd => sd.RecordedAt >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(sd => sd.RecordedAt <= toDate.Value);

        return await Task.FromResult(
            query.OrderByDescending(sd => sd.RecordedAt)
                 .Take(limit)
                 .ToList());
    }

    public async Task<List<SensorDatum>> GetLatestReadingsAsync(Guid? experimentId = null, Guid? batchId = null)
    {
        var query = _context.SensorData.AsQueryable();

        if (experimentId.HasValue)
            query = query.Where(sd => sd.ExperimentId == experimentId.Value);

        if (batchId.HasValue)
            query = query.Where(sd => sd.BatchId == batchId.Value);

        var latestPerSensor = query
            .GroupBy(sd => sd.SensorId)
            .Select(g => new { SensorId = g.Key, LastDatum = g.OrderByDescending(sd => sd.RecordedAt).First() })
            .ToList();

        var result = latestPerSensor.Select(x => x.LastDatum).ToList();
        return await Task.FromResult(result);
    }

    public async Task<SensorDatum?> GetLatestReadingBySensorAsync(Guid sensorId)
    {
        return await Task.FromResult(
            _context.SensorData
                .Where(sd => sd.SensorId == sensorId)
                .OrderByDescending(sd => sd.RecordedAt)
                .FirstOrDefault());
    }

    public async Task<List<Sensor>> GetActiveSensorsAsync()
    {
        return await Task.FromResult(_context.Sensors.ToList());
    }
}
