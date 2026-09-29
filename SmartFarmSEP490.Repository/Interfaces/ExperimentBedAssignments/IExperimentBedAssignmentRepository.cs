using M = SmartFarmSEP490.Model;

namespace SmartFarmSEP490.Repository.Interfaces.ExperimentBedAssignments;

public interface IExperimentBedAssignmentRepository
{
    Task<M.ExperimentBedAssignment?> GetByIdAsync(Guid id);
    Task<List<M.ExperimentBedAssignment>> GetByExperimentAsync(Guid experimentId);
    Task<M.ExperimentBedAssignment?> GetActiveByBedAsync(Guid bedId, Guid? currentRequestId = null);
    Task<List<M.ExperimentBedAssignment>> GetByBedAsync(Guid bedId);
    Task<M.ExperimentBedAssignment> CreateAsync(M.ExperimentBedAssignment entity);
    Task CreateRangeAsync(List<M.ExperimentBedAssignment> entities);
    Task UpdateAsync(M.ExperimentBedAssignment entity);
    Task DeleteAsync(Guid id);
    Task<List<M.ExperimentBedAssignment>> GetByRequestAsync(Guid requestId);
    Task AssignBedsToExperimentAsync(Guid requestId, Guid experimentId);
    Task UpdateOrCreateAssignmentAsync(Guid requestId, Guid bedId, Guid? experimentId, DateOnly assignedFrom, string? purpose);
    Task ReleaseBedsAsync(Guid experimentId);

    /// <summary>
    /// Lấy toàn bộ bed assignments (cả active và released) cho một experiment,
    /// kèm Include sẵn Bed/Area/Group để FE render. Dùng cho endpoint GET /bed-history.
    /// </summary>
    Task<List<M.ExperimentBedAssignment>> GetHistoryByExperimentAsync(Guid experimentId);

    Task<List<Guid>> GetAvailableBedIdsByFarmAsync(Guid farmId);
    Task UpdateGroupAssignmentAsync(Guid assignmentId, Guid? groupId, int? replicateIndex);
    Task UpdateRangeAsync(IEnumerable<M.ExperimentBedAssignment> entities);
}
