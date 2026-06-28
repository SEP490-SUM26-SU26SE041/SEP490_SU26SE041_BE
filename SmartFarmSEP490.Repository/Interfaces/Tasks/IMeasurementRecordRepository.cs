using SmartFarmSEP490.Model;
using Task = System.Threading.Tasks.Task;

namespace SmartFarmSEP490.Repository.Interfaces.Tasks;

public interface IMeasurementRecordRepository
{
    Task<MeasurementRecord?> GetByIdAsync(Guid id);
    Task<List<MeasurementRecord>> GetByBatchIdAsync(Guid batchId);
    Task<MeasurementRecord> CreateAsync(MeasurementRecord entity);
    Task UpdateAsync(MeasurementRecord entity);
    Task DeleteAsync(Guid id);
}
