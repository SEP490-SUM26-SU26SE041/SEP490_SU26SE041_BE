using M = SmartFarmSEP490.Model;

namespace SmartFarmSEP490.Repository.Interfaces.CropVarieties;

public interface ICropVarietyRepository
{
    Task<M.CropVariety?> GetByIdAsync(Guid id);
    Task<List<M.CropVariety>> GetByCropAsync(Guid cropId);
    Task<M.CropVariety> CreateAsync(M.CropVariety entity);
    Task UpdateAsync(M.CropVariety entity);
    Task DeleteAsync(Guid id);
}
