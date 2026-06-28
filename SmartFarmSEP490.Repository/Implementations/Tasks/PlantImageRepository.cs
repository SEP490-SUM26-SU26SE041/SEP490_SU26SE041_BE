using Microsoft.EntityFrameworkCore;
using SmartFarmSEP490.Model;
using SmartFarmSEP490.Repository.DbContexts;
using SmartFarmSEP490.Repository.Interfaces.Tasks;
using Task = System.Threading.Tasks.Task;

namespace SmartFarmSEP490.Repository.Implementations.Tasks;

public class PlantImageRepository : IPlantImageRepository
{
    private readonly SmartFarmDbContext _context;
    public PlantImageRepository(SmartFarmDbContext context) => _context = context;

    public async Task<PlantImage?> GetByIdAsync(Guid id) =>
        await _context.PlantImages
            .Include(p => p.UploadedByNavigation)
            .FirstOrDefaultAsync(p => p.Id == id);

    public async Task<List<PlantImage>> GetByTaskReportIdAsync(Guid taskReportId) =>
        await _context.PlantImages
            .Include(p => p.UploadedByNavigation)
            .Where(p => p.TaskReportId == taskReportId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

    public async Task<List<PlantImage>> GetByBatchIdAsync(Guid batchId) =>
        await _context.PlantImages
            .Include(p => p.UploadedByNavigation)
            .Where(p => p.BatchId == batchId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

    public async Task<PlantImage> CreateAsync(PlantImage entity)
    {
        entity.CreatedAt = DateTime.UtcNow;
        await _context.PlantImages.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task DeleteAsync(Guid id)
    {
        var e = await _context.PlantImages.FindAsync(id);
        if (e != null) { _context.PlantImages.Remove(e); await _context.SaveChangesAsync(); }
    }
}
