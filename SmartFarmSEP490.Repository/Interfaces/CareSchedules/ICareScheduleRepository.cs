using M = SmartFarmSEP490.Model;

namespace SmartFarmSEP490.Repository.Interfaces.CareSchedules;

public interface ICareScheduleRepository
{
    Task<M.CareSchedule?> GetByIdAsync(Guid id);
    Task<List<M.CareSchedule>> GetByExperimentAsync(Guid experimentId);
    Task<List<M.CareSchedule>> GetByStageAsync(Guid stageId);
    Task<List<M.CareSchedule>> GetByStageAndBatchAsync(Guid stageId, Guid batchId);
    Task<M.CareSchedule> CreateAsync(M.CareSchedule entity);
    Task UpdateAsync(M.CareSchedule entity);
    Task DeleteAsync(Guid id);
}
