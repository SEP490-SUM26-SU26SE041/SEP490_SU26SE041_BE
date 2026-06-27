using SmartFarmSEP490.Model;
using Task = System.Threading.Tasks.Task;

namespace SmartFarmSEP490.Repository.Interfaces.Tasks;

public interface ITaskReportRepository
{
    Task<TaskReport?> GetByIdAsync(Guid id);
    Task<List<TaskReport>> GetByTaskIdAsync(Guid taskId);
    Task<List<TaskReport>> GetByBatchIdAsync(Guid batchId);
    Task<TaskReport> CreateAsync(TaskReport entity);
    Task UpdateAsync(TaskReport entity);
    Task DeleteAsync(Guid id);
}
