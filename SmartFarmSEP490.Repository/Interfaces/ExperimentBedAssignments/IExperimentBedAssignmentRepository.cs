using M = SmartFarmSEP490.Model;

namespace SmartFarmSEP490.Repository.Interfaces.ExperimentBedAssignments;

public interface IExperimentBedAssignmentRepository
{
    Task<M.ExperimentBedAssignment?> GetByIdAsync(Guid id);
    Task<List<M.ExperimentBedAssignment>> GetByExperimentAsync(Guid experimentId);
    Task<M.ExperimentBedAssignment?> GetActiveByBedAsync(Guid bedId);
    Task<List<M.ExperimentBedAssignment>> GetByBedAsync(Guid bedId);
    Task<M.ExperimentBedAssignment> CreateAsync(M.ExperimentBedAssignment entity);
    Task CreateRangeAsync(List<M.ExperimentBedAssignment> entities);
    Task UpdateAsync(M.ExperimentBedAssignment entity);
    Task DeleteAsync(Guid id);
    Task<List<M.ExperimentBedAssignment>> GetByRequestAsync(Guid requestId);
    Task AssignBedsToExperimentAsync(Guid requestId, Guid experimentId);
    Task ReleaseBedsAsync(Guid experimentId);
    Task<List<Guid>> GetAvailableBedIdsByFarmAsync(Guid farmId);
}
