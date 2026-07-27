using SmartFarmSEP490.Model;
using Task = System.Threading.Tasks.Task;

namespace SmartFarmSEP490.Repository.Interfaces.Experiments;

public interface IExperimentReportRepository
{
    Task<List<ExperimentReport>> GetAllAsync();
    Task<List<ExperimentReport>> GetByExperimentAsync(Guid experimentId);
    Task<ExperimentReport?> GetByIdAsync(Guid id);
    Task<ExperimentReport> AddAsync(ExperimentReport report);
    Task UpdateAsync(ExperimentReport report);
    Task DeleteAsync(Guid id);
}
