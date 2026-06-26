using SmartFarmSEP490.Model;

namespace SmartFarmSEP490.Repository.Interfaces.Tasks;

public interface ITaskRepository
{
    System.Threading.Tasks.Task<Model.Task?> GetByIdAsync(Guid id);
    System.Threading.Tasks.Task<System.Collections.Generic.List<Model.Task>> GetByExperimentAsync(Guid experimentId);
    System.Threading.Tasks.Task<System.Collections.Generic.List<Model.Task>> GetByAssigneeAsync(Guid assigneeId);
    System.Threading.Tasks.Task<System.Collections.Generic.List<Model.Task>> GetAllAsync();
    System.Threading.Tasks.Task<Model.Task> AddAsync(Model.Task task);
    System.Threading.Tasks.Task UpdateAsync(Model.Task task);
    System.Threading.Tasks.Task DeleteAsync(Guid id);
}
