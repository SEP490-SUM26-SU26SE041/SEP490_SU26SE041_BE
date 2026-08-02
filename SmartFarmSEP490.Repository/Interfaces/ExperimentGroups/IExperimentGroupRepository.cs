using M = SmartFarmSEP490.Model;

namespace SmartFarmSEP490.Repository.Interfaces.ExperimentGroups;

public interface IExperimentGroupRepository
{
    Task<M.ExperimentGroup?> GetByIdAsync(Guid id);
    Task<List<M.ExperimentGroup>> GetByExperimentAsync(Guid experimentId);
    Task<M.ExperimentGroup> CreateAsync(M.ExperimentGroup entity);
    Task AddRangeAsync(IEnumerable<M.ExperimentGroup> entities);
    Task UpdateAsync(M.ExperimentGroup entity);
    Task DeleteAsync(Guid id);
}
