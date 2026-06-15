using Microsoft.EntityFrameworkCore;
using M = SmartFarmSEP490.Model;
using SmartFarmSEP490.Repository.DbContexts;
using SmartFarmSEP490.Repository.Interfaces.Crops;
using Task = System.Threading.Tasks.Task;

namespace SmartFarmSEP490.Repository.Implementations.Crops;

public class CropRepository : ICropRepository
{
    private readonly SmartFarmDbContext _context;
    public CropRepository(SmartFarmDbContext context) => _context = context;

    public async Task<M.Crop?> GetByIdAsync(Guid id) => await _context.Crops.FindAsync(id);

    public async Task<M.Crop?> GetByIdWithVarietiesAsync(Guid id) =>
        await _context.Crops.Include(c => c.CropVarieties).FirstOrDefaultAsync(c => c.Id == id);

    public async Task<List<M.Crop>> GetAllAsync() =>
        await _context.Crops.Include(c => c.CropVarieties)
            .Where(c => c.DeletedAt == null).OrderBy(c => c.CropName).ToListAsync();

    public async Task<M.Crop> CreateAsync(M.Crop entity)
    {
        entity.CreatedAt = DateTime.UtcNow;
        await _context.Crops.AddAsync(entity); await _context.SaveChangesAsync(); return entity;
    }

    public async Task UpdateAsync(M.Crop entity)
    {
        _context.Crops.Update(entity); await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var e = await _context.Crops.FindAsync(id);
        if (e != null) { e.DeletedAt = DateTime.UtcNow; await UpdateAsync(e); }
    }
}
