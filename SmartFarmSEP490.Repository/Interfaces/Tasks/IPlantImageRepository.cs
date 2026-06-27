using SmartFarmSEP490.Model;
using Task = System.Threading.Tasks.Task;

namespace SmartFarmSEP490.Repository.Interfaces.Tasks;

public interface IPlantImageRepository
{
    Task<PlantImage?> GetByIdAsync(Guid id);
    Task<List<PlantImage>> GetByTaskReportIdAsync(Guid taskReportId);
    Task<List<PlantImage>> GetByBatchIdAsync(Guid batchId);
    Task<PlantImage> CreateAsync(PlantImage entity);
    Task DeleteAsync(Guid id);
}
