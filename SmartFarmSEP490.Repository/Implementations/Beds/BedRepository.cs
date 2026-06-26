using Microsoft.EntityFrameworkCore;
using M = SmartFarmSEP490.Model;
using SmartFarmSEP490.Repository.DbContexts;
using SmartFarmSEP490.Repository.Interfaces.Beds;
using Task = System.Threading.Tasks.Task;

namespace SmartFarmSEP490.Repository.Implementations.Beds;

public class BedRepository : IBedRepository
{
    private readonly SmartFarmDbContext _context;
    public BedRepository(SmartFarmDbContext context) => _context = context;

    public async Task<M.Bed?> GetByIdAsync(Guid id) =>
        await _context.Beds.Include(b => b.Area).ThenInclude(a => a.Farm)
            .FirstOrDefaultAsync(b => b.Id == id);

    public async Task<List<M.Bed>> GetByAreaAsync(Guid areaId) =>
        await _context.Beds.Where(b => b.AreaId == areaId && b.DeletedAt == null)
            .OrderBy(b => b.BedCode).ToListAsync();

    public async Task<List<M.Bed>> GetByIdsAsync(List<Guid> bedIds) =>
        await _context.Beds.Where(b => bedIds.Contains(b.Id)).ToListAsync();

    public async Task<M.Bed> CreateAsync(M.Bed entity)
    {
        entity.CreatedAt = DateTime.UtcNow; entity.UpdatedAt = DateTime.UtcNow;
        await _context.Beds.AddAsync(entity); await _context.SaveChangesAsync(); return entity;
    }

    public async Task UpdateAsync(M.Bed entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _context.Beds.Update(entity); await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var e = await _context.Beds.FindAsync(id);
        if (e != null) { e.DeletedAt = DateTime.UtcNow; await UpdateAsync(e); }
    }
}
