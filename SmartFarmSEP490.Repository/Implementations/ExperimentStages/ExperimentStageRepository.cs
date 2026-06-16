using Microsoft.EntityFrameworkCore;
using M = SmartFarmSEP490.Model;
using SmartFarmSEP490.Repository.DbContexts;
using SmartFarmSEP490.Repository.Interfaces.ExperimentStages;
using Task = System.Threading.Tasks.Task;

namespace SmartFarmSEP490.Repository.Implementations.ExperimentStages;

public class ExperimentStageRepository : IExperimentStageRepository
{
    private readonly SmartFarmDbContext _context;
    public ExperimentStageRepository(SmartFarmDbContext context) => _context = context;

    public async Task<M.ExperimentStage?> GetByIdAsync(Guid id) => await _context.ExperimentStages.FindAsync(id);

    public async Task<List<M.ExperimentStage>> GetByExperimentAsync(Guid experimentId) =>
        await _context.ExperimentStages.Where(s => s.ExperimentId == experimentId).OrderBy(s => s.StageOrder).ToListAsync();

    public async Task<M.ExperimentStage> CreateAsync(M.ExperimentStage entity)
    {
        entity.CreatedAt = DateTime.UtcNow; entity.UpdatedAt = DateTime.UtcNow;
        await _context.ExperimentStages.AddAsync(entity); await _context.SaveChangesAsync(); return entity;
    }

    public async Task UpdateAsync(M.ExperimentStage entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _context.ExperimentStages.Update(entity); await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var e = await _context.ExperimentStages.FindAsync(id);
        if (e != null) { _context.ExperimentStages.Remove(e); await _context.SaveChangesAsync(); }
    }
}
