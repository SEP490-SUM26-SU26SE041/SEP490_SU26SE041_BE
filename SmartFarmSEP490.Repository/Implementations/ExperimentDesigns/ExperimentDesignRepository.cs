using Microsoft.EntityFrameworkCore;
using M = SmartFarmSEP490.Model;
using SmartFarmSEP490.Repository.DbContexts;
using SmartFarmSEP490.Repository.Interfaces.ExperimentDesigns;
using Task = System.Threading.Tasks.Task;

namespace SmartFarmSEP490.Repository.Implementations.ExperimentDesigns;

public class ExperimentDesignRepository : IExperimentDesignRepository
{
    private readonly SmartFarmDbContext _context;
    public ExperimentDesignRepository(SmartFarmDbContext context) => _context = context;

    public async Task<M.ExperimentDesign?> GetByExperimentAsync(Guid experimentId) =>
        await _context.ExperimentDesigns.FirstOrDefaultAsync(d => d.ExperimentId == experimentId);

    public async Task<M.ExperimentDesign> CreateAsync(M.ExperimentDesign entity)
    {
        await _context.ExperimentDesigns.AddAsync(entity); await _context.SaveChangesAsync(); return entity;
    }

    public async Task UpdateAsync(M.ExperimentDesign entity)
    {
        _context.ExperimentDesigns.Update(entity); await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid experimentId)
    {
        var e = await _context.ExperimentDesigns.FirstOrDefaultAsync(d => d.ExperimentId == experimentId);
        if (e != null) { _context.ExperimentDesigns.Remove(e); await _context.SaveChangesAsync(); }
    }
}
