using M = SmartFarmSEP490.Model;

namespace SmartFarmSEP490.Repository.Interfaces.Farms;

public interface IFarmRepository
{
    Task<M.Farm?> GetByIdAsync(Guid id);
    Task<M.Farm?> GetByIdWithDetailsAsync(Guid id);
    Task<List<M.Farm>> GetAllAsync();
    Task<M.Farm?> GetByCodeAsync(string code);
    Task<List<M.Farm>> GetByManagerAsync(Guid managerId);
    Task<M.Farm> CreateAsync(M.Farm entity);
    Task UpdateAsync(M.Farm entity);
    Task DeleteAsync(Guid id);
}
