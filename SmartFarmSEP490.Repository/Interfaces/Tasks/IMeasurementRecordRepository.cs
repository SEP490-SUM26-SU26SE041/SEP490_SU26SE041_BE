using SmartFarmSEP490.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace SmartFarmSEP490.Repository.Interfaces.Tasks;

public interface IMeasurementRecordRepository
{
    Task<MeasurementRecord?> GetByIdAsync(Guid id, bool includeDeleted = false);
    Task<List<MeasurementRecord>> GetByBatchIdAsync(Guid batchId, bool includeDeleted = false);
    Task<List<MeasurementRecord>> GetByExperimentIdAsync(Guid experimentId, bool includeDeleted = false);
    Task<List<MeasurementRecord>> GetByStageIdAsync(Guid stageId, bool includeDeleted = false);
    Task<List<MeasurementDefinition>> GetDefinitionsByIdsAsync(IEnumerable<Guid> ids);
    Task<MeasurementRecord> CreateAsync(MeasurementRecord entity);
    Task<List<MeasurementRecord>> CreateBulkAsync(IEnumerable<MeasurementRecord> entities);
    Task UpdateAsync(MeasurementRecord entity);
    Task SoftDeleteAsync(Guid id);
}
