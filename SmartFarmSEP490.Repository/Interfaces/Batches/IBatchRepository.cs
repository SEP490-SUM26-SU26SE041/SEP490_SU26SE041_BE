using M = SmartFarmSEP490.Model;

namespace SmartFarmSEP490.Repository.Interfaces.Batches;

public interface IBatchRepository
{
    Task<M.Batch?> GetByIdAsync(Guid id);
    Task<List<M.Batch>> GetByExperimentAsync(Guid experimentId);
    Task<M.Batch?> GetByCodeAsync(string code);
    Task<M.Batch> CreateAsync(M.Batch entity);
    Task UpdateAsync(M.Batch entity);
    Task DeleteAsync(Guid id);
}
