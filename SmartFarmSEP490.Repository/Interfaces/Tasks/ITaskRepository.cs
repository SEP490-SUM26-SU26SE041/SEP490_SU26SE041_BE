using SmartFarmSEP490.Model;
using SmartFarmSEP490.Model.DTOs;
using Task = System.Threading.Tasks.Task;

namespace SmartFarmSEP490.Repository.Interfaces.Tasks;

public interface ITaskRepository
{
    Task<Model.Task?> GetByIdAsync(Guid id);
    Task<List<Model.Task>> GetByExperimentAsync(Guid experimentId);
    Task<List<Model.Task>> GetByStageAsync(Guid stageId);
    Task<List<Model.Task>> GetByBatchAsync(Guid batchId);
    Task<List<Model.Task>> GetByAssigneeAsync(Guid assigneeId);
    Task<List<Model.Task>> GetAllAsync();
    Task<List<Model.Task>> GetTodayTasksAsync(Guid assigneeId);
    Task<List<Model.Task>> GetUpcomingTasksAsync(Guid assigneeId, int days);
    Task<List<Model.Task>> GetOverdueTasksAsync(Guid assigneeId);
    Task<List<Model.Task>> GetResearcherCreatedTasksAsync(ResearcherCreatedTaskFilterDto filter);
    Task<Model.Task> AddAsync(Model.Task task);
    Task UpdateAsync(Model.Task task);
    Task DeleteAsync(Guid id);
}
