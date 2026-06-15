using M = SmartFarmSEP490.Model;

namespace SmartFarmSEP490.Repository.Interfaces.ExperimentDesigns;

public interface IExperimentDesignRepository
{
    Task<M.ExperimentDesign?> GetByExperimentAsync(Guid experimentId);
    Task<M.ExperimentDesign> CreateAsync(M.ExperimentDesign entity);
    Task UpdateAsync(M.ExperimentDesign entity);
    Task DeleteAsync(Guid experimentId);
}
