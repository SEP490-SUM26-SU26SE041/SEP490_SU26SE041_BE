using M = SmartFarmSEP490.Model;

namespace SmartFarmSEP490.Repository.Interfaces.Areas;

public interface IAreaRepository
{
    Task<M.Area?> GetByIdAsync(Guid id);
    Task<List<M.Area>> GetByFarmAsync(Guid farmId);
    Task<M.Area> CreateAsync(M.Area entity);
    Task UpdateAsync(M.Area entity);
    Task DeleteAsync(Guid id);
}
