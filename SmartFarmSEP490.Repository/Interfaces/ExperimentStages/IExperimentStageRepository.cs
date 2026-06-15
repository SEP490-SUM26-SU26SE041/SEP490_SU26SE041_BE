using M = SmartFarmSEP490.Model;

namespace SmartFarmSEP490.Repository.Interfaces.ExperimentStages;

public interface IExperimentStageRepository
{
    Task<M.ExperimentStage?> GetByIdAsync(Guid id);
    Task<List<M.ExperimentStage>> GetByExperimentAsync(Guid experimentId);
    Task<M.ExperimentStage> CreateAsync(M.ExperimentStage entity);
    Task UpdateAsync(M.ExperimentStage entity);
    Task DeleteAsync(Guid id);
}
