using Microsoft.EntityFrameworkCore;
using M = SmartFarmSEP490.Model;
using SmartFarmSEP490.Repository.DbContexts;
using SmartFarmSEP490.Repository.Interfaces.CropVarieties;
using Task = System.Threading.Tasks.Task;

namespace SmartFarmSEP490.Repository.Implementations.CropVarieties;

public class CropVarietyRepository : ICropVarietyRepository
{
    private readonly SmartFarmDbContext _context;
    public CropVarietyRepository(SmartFarmDbContext context) => _context = context;

    public async Task<M.CropVariety?> GetByIdAsync(Guid id) =>
        await _context.CropVarieties.Include(cv => cv.Crop).FirstOrDefaultAsync(cv => cv.Id == id);

    public async Task<List<M.CropVariety>> GetByCropAsync(Guid cropId) =>
        await _context.CropVarieties.Where(cv => cv.CropId == cropId && cv.DeletedAt == null)
            .OrderBy(cv => cv.VarietyName).ToListAsync();

    public async Task<M.CropVariety> CreateAsync(M.CropVariety entity)
    {
        entity.CreatedAt = DateTime.UtcNow;
        await _context.CropVarieties.AddAsync(entity); await _context.SaveChangesAsync(); return entity;
    }

    public async Task UpdateAsync(M.CropVariety entity)
    {
        _context.CropVarieties.Update(entity); await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var e = await _context.CropVarieties.FindAsync(id);
        if (e != null) { e.DeletedAt = DateTime.UtcNow; await UpdateAsync(e); }
    }
}
