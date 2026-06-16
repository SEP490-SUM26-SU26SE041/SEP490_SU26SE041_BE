using Microsoft.EntityFrameworkCore;
using M = SmartFarmSEP490.Model;
using SmartFarmSEP490.Repository.DbContexts;
using SmartFarmSEP490.Repository.Interfaces.Farms;
using Task = System.Threading.Tasks.Task;

namespace SmartFarmSEP490.Repository.Implementations.Farms;

public class FarmRepository : IFarmRepository
{
    private readonly SmartFarmDbContext _context;
    public FarmRepository(SmartFarmDbContext context) => _context = context;

    public async Task<M.Farm?> GetByIdAsync(Guid id) => await _context.Farms.FindAsync(id);

    public async Task<M.Farm?> GetByIdWithDetailsAsync(Guid id) =>
        await _context.Farms.Include(f => f.Manager)
            .Include(f => f.Areas).ThenInclude(a => a.Beds)
            .FirstOrDefaultAsync(f => f.Id == id);

    public async Task<List<M.Farm>> GetAllAsync() =>
        await _context.Farms.Include(f => f.Manager).OrderBy(f => f.FarmName).ToListAsync();

    public async Task<M.Farm?> GetByCodeAsync(string code) =>
        await _context.Farms.FirstOrDefaultAsync(f => f.FarmCode == code);

    public async Task<List<M.Farm>> GetByManagerAsync(Guid managerId) =>
        await _context.Farms.Include(f => f.Manager)
            .Where(f => f.ManagerId == managerId)
            .OrderBy(f => f.FarmName)
            .ToListAsync();

    public async Task<M.Farm> CreateAsync(M.Farm entity)
    {
        entity.CreatedAt = DateTime.UtcNow; entity.UpdatedAt = DateTime.UtcNow;
        await _context.Farms.AddAsync(entity); await _context.SaveChangesAsync(); return entity;
    }

    public async Task UpdateAsync(M.Farm entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _context.Farms.Update(entity); await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var e = await _context.Farms.FindAsync(id);
        if (e != null) { e.DeletedAt = DateTime.UtcNow; await UpdateAsync(e); }
    }
}
