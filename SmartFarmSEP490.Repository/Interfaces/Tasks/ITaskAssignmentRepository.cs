using SmartFarmSEP490.Model;

namespace SmartFarmSEP490.Repository.Interfaces.Tasks;

public interface ITaskAssignmentRepository
{
    System.Threading.Tasks.Task<TaskAssignment?> GetByIdAsync(Guid id);
    System.Threading.Tasks.Task<System.Collections.Generic.List<TaskAssignment>> GetByTaskIdAsync(Guid taskId);
    System.Threading.Tasks.Task<System.Collections.Generic.List<TaskAssignment>> GetByAssigneeAsync(Guid assigneeId);
    System.Threading.Tasks.Task<TaskAssignment?> GetActiveByTaskAndAssigneeAsync(Guid taskId, Guid assigneeId);
    System.Threading.Tasks.Task<TaskAssignment> AddAsync(TaskAssignment assignment);
    System.Threading.Tasks.Task UpdateAsync(TaskAssignment assignment);
    System.Threading.Tasks.Task DeleteAsync(Guid id);
    System.Threading.Tasks.Task<bool> HasActiveAssignmentAsync(Guid taskId);
}
