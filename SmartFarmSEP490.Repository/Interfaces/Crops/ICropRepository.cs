using M = SmartFarmSEP490.Model;

namespace SmartFarmSEP490.Repository.Interfaces.Crops;

public interface ICropRepository
{
    Task<M.Crop?> GetByIdAsync(Guid id);
    Task<M.Crop?> GetByIdWithVarietiesAsync(Guid id);
    Task<List<M.Crop>> GetAllAsync();
    Task<M.Crop> CreateAsync(M.Crop entity);
    Task UpdateAsync(M.Crop entity);
    Task DeleteAsync(Guid id);
}
