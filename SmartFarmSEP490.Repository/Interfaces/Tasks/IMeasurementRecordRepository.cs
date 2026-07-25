using SmartFarmSEP490.Model;
using Task = System.Threading.Tasks.Task;

namespace SmartFarmSEP490.Repository.Interfaces.Tasks;

public interface IMeasurementRecordRepository
{
    Task<MeasurementRecord?> GetByIdAsync(Guid id, bool includeDeleted = false);
    Task<List<MeasurementRecord>> GetByBatchIdAsync(Guid batchId, bool includeDeleted = false);
    Task<List<MeasurementRecord>> GetByExperimentIdAsync(Guid experimentId, bool includeDeleted = false);
    Task<List<MeasurementRecord>> GetByStageIdAsync(Guid stageId, bool includeDeleted = false);
    Task<MeasurementRecord> CreateAsync(MeasurementRecord entity);
    Task UpdateAsync(MeasurementRecord entity);
    Task SoftDeleteAsync(Guid id);
}
