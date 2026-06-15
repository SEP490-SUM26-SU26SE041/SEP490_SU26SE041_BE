using M = SmartFarmSEP490.Model;

namespace SmartFarmSEP490.Repository.Interfaces.Experiments;

public interface IExperimentRepository
{
    Task<M.Experiment?> GetByIdAsync(Guid id);
    Task<M.Experiment?> GetByIdWithDetailsAsync(Guid id);
    Task<List<M.Experiment>> GetAllAsync();
    Task<List<M.Experiment>> GetByResearcherAsync(Guid researcherId);
    Task<List<M.Experiment>> GetByFarmAsync(Guid farmId);
    Task<M.Experiment?> GetByCodeAsync(string code);
    Task<M.Experiment> CreateAsync(M.Experiment entity);
    Task UpdateAsync(M.Experiment entity);
    Task DeleteAsync(Guid id);
}
