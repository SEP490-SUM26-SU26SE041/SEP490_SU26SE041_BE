using Microsoft.EntityFrameworkCore;
using M = SmartFarmSEP490.Model;
using SmartFarmSEP490.Repository.DbContexts;
using SmartFarmSEP490.Repository.Interfaces.Areas;
using Task = System.Threading.Tasks.Task;

namespace SmartFarmSEP490.Repository.Implementations.Areas;

public class AreaRepository : IAreaRepository
{
    private readonly SmartFarmDbContext _context;
    public AreaRepository(SmartFarmDbContext context) => _context = context;

    public async Task<M.Area?> GetByIdAsync(Guid id) => await _context.Areas.FindAsync(id);

    public async Task<List<M.Area>> GetByFarmAsync(Guid farmId) =>
        await _context.Areas
            .Include(a => a.Beds).ThenInclude(b => b.ExperimentBedAssignments)
            .Where(a => a.FarmId == farmId && a.DeletedAt == null)
            .OrderBy(a => a.AreaName)
            .ToListAsync();

    public async Task<M.Area> CreateAsync(M.Area entity)
    {
        entity.CreatedAt = DateTime.UtcNow; entity.UpdatedAt = DateTime.UtcNow;
        await _context.Areas.AddAsync(entity); await _context.SaveChangesAsync(); return entity;
    }

    public async Task UpdateAsync(M.Area entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _context.Areas.Update(entity); await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var e = await _context.Areas.FindAsync(id);
        if (e != null) { e.DeletedAt = DateTime.UtcNow; await UpdateAsync(e); }
    }
}
