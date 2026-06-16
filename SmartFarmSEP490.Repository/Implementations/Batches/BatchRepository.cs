using Microsoft.EntityFrameworkCore;
using M = SmartFarmSEP490.Model;
using SmartFarmSEP490.Repository.DbContexts;
using SmartFarmSEP490.Repository.Interfaces.Batches;
using Task = System.Threading.Tasks.Task;

namespace SmartFarmSEP490.Repository.Implementations.Batches;

public class BatchRepository : IBatchRepository
{
    private readonly SmartFarmDbContext _context;
    public BatchRepository(SmartFarmDbContext context) => _context = context;

    public async Task<M.Batch?> GetByIdAsync(Guid id) =>
        await _context.Batches
            .Include(b => b.Experiment)
            .Include(b => b.ExperimentBedAssignment).ThenInclude(eba => eba!.Bed).ThenInclude(b => b.Area)
            .Include(b => b.Group).Include(b => b.CropVariety)
            .FirstOrDefaultAsync(b => b.Id == id);

    public async Task<List<M.Batch>> GetByExperimentAsync(Guid experimentId) =>
        await _context.Batches
            .Include(b => b.ExperimentBedAssignment).ThenInclude(eba => eba!.Bed).ThenInclude(b => b.Area)
            .Include(b => b.Group).Include(b => b.CropVariety)
            .Where(b => b.ExperimentId == experimentId && b.DeletedAt == null).OrderBy(b => b.BatchCode).ToListAsync();

    public async Task<M.Batch?> GetByCodeAsync(string code) =>
        await _context.Batches.FirstOrDefaultAsync(b => b.BatchCode == code);

    public async Task<M.Batch> CreateAsync(M.Batch entity)
    {
        entity.CreatedAt = DateTime.UtcNow;
        await _context.Batches.AddAsync(entity); await _context.SaveChangesAsync(); return entity;
    }

    public async Task UpdateAsync(M.Batch entity)
    {
        _context.Batches.Update(entity); await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var e = await _context.Batches.FindAsync(id);
        if (e != null) { e.DeletedAt = DateTime.UtcNow; await UpdateAsync(e); }
    }
}
