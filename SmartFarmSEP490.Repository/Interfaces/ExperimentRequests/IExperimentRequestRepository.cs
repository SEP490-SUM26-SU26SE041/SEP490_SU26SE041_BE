using M = SmartFarmSEP490.Model;

namespace SmartFarmSEP490.Repository.Interfaces.ExperimentRequests;

public interface IExperimentRequestRepository
{
    Task<M.ExperimentRequest?> GetByIdAsync(Guid id);
    Task<M.ExperimentRequest?> GetByIdWithDetailsAsync(Guid id);
    Task<List<M.ExperimentRequest>> GetAllAsync();
    Task<List<M.ExperimentRequest>> GetByResearcherAsync(Guid researcherId);
    Task<List<M.ExperimentRequest>> GetByFarmAsync(Guid farmId);
    Task<List<M.ExperimentRequest>> GetByStatusAsync(string status);
    Task<List<M.ExperimentRequest>> GetByManagerAsync(Guid managerId, M.Enums.RequestStatus? status);
    Task<M.ExperimentRequest> CreateAsync(M.ExperimentRequest entity);
    Task UpdateAsync(M.ExperimentRequest entity);
    Task DeleteAsync(Guid id);
}
