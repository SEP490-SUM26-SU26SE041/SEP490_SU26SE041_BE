using Microsoft.EntityFrameworkCore;
using M = SmartFarmSEP490.Model;
using SmartFarmSEP490.Repository.DbContexts;
using SmartFarmSEP490.Repository.Interfaces.Experiments;
using Task = System.Threading.Tasks.Task;

namespace SmartFarmSEP490.Repository.Implementations.Experiments;

public class ExperimentRepository : IExperimentRepository
{
    private readonly SmartFarmDbContext _context;
    public ExperimentRepository(SmartFarmDbContext context) => _context = context;

    public async Task<M.Experiment?> GetByIdAsync(Guid id) => await _context.Experiments.FindAsync(id);

    public async Task<M.Experiment?> GetByIdWithDetailsAsync(Guid id) =>
        await _context.Experiments
            .Include(e => e.Researcher).Include(e => e.Farm)
            .Include(e => e.CropVariety).Include(e => e.ProcedureTemplate)
            .Include(e => e.ExperimentStages.OrderBy(s => s.StageOrder))
            .Include(e => e.ExperimentGroups)
            .Include(e => e.MeasurementDefinitions)
            .Include(e => e.ExperimentDesign)
            .Include(e => e.ExperimentBedAssignments).ThenInclude(eba => eba.Bed).ThenInclude(b => b.Area)
            .FirstOrDefaultAsync(e => e.Id == id);

    public async Task<List<M.Experiment>> GetAllAsync() =>
        await _context.Experiments.Include(e => e.Researcher).Include(e => e.Farm)
            .OrderByDescending(e => e.CreatedAt).ToListAsync();

    public async Task<List<M.Experiment>> GetByResearcherAsync(Guid researcherId) =>
        await _context.Experiments.Include(e => e.Farm)
            .Where(e => e.ResearcherId == researcherId).OrderByDescending(e => e.CreatedAt).ToListAsync();

    public async Task<List<M.Experiment>> GetByFarmAsync(Guid farmId) =>
        await _context.Experiments.Include(e => e.Researcher)
            .Where(e => e.FarmId == farmId).OrderByDescending(e => e.CreatedAt).ToListAsync();

    public async Task<M.Experiment?> GetByCodeAsync(string code) =>
        await _context.Experiments.FirstOrDefaultAsync(e => e.ExperimentCode == code);

    public async Task<M.Experiment> CreateAsync(M.Experiment entity)
    {
        entity.CreatedAt = DateTime.UtcNow; entity.UpdatedAt = DateTime.UtcNow;
        await _context.Experiments.AddAsync(entity); await _context.SaveChangesAsync(); return entity;
    }

    public async Task<M.Experiment> CreateWithStagesAsync(M.Experiment entity, IEnumerable<M.ExperimentStage> stages)
    {
        // KHONG mo transaction o day vi method nay duoc goi tu CreateFromRequestAsync 
        // da co transaction o Service layer
        entity.CreatedAt = DateTime.UtcNow; entity.UpdatedAt = DateTime.UtcNow;
        await _context.Experiments.AddAsync(entity);
        await _context.SaveChangesAsync();
        var now = DateTime.UtcNow;
        foreach (var s in stages)
        {
            s.ExperimentId = entity.Id;
            s.CreatedAt = now;
            s.UpdatedAt = now;
        }
        await _context.ExperimentStages.AddRangeAsync(stages);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<M.ExperimentDesign> CreateDesignAsync(M.ExperimentDesign entity)
    {
        await _context.ExperimentDesigns.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(M.Experiment entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _context.Experiments.Update(entity); await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var e = await _context.Experiments.FindAsync(id);
        if (e != null) { e.DeletedAt = DateTime.UtcNow; await UpdateAsync(e); }
    }
}
