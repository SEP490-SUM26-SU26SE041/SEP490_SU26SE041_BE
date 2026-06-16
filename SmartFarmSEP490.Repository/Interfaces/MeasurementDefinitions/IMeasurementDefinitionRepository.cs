using M = SmartFarmSEP490.Model;

namespace SmartFarmSEP490.Repository.Interfaces.MeasurementDefinitions;

public interface IMeasurementDefinitionRepository
{
    Task<M.MeasurementDefinition?> GetByIdAsync(Guid id);
    Task<List<M.MeasurementDefinition>> GetByExperimentAsync(Guid experimentId);
    Task<M.MeasurementDefinition> CreateAsync(M.MeasurementDefinition entity);
    Task UpdateAsync(M.MeasurementDefinition entity);
    Task DeleteAsync(Guid id);
}
