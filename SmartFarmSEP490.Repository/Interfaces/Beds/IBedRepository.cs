using M = SmartFarmSEP490.Model;

namespace SmartFarmSEP490.Repository.Interfaces.Beds;

public interface IBedRepository
{
    Task<M.Bed?> GetByIdAsync(Guid id);
    Task<List<M.Bed>> GetByAreaAsync(Guid areaId);
    Task<List<M.Bed>> GetAvailableByFarmAsync(Guid farmId);
    Task<M.Bed> CreateAsync(M.Bed entity);
    Task UpdateAsync(M.Bed entity);
    Task DeleteAsync(Guid id);
}
