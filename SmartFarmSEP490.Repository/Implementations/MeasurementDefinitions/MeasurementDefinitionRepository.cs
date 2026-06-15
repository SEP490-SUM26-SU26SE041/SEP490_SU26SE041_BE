using Microsoft.EntityFrameworkCore;
using M = SmartFarmSEP490.Model;
using SmartFarmSEP490.Repository.DbContexts;
using SmartFarmSEP490.Repository.Interfaces.MeasurementDefinitions;
using Task = System.Threading.Tasks.Task;

namespace SmartFarmSEP490.Repository.Implementations.MeasurementDefinitions;

public class MeasurementDefinitionRepository : IMeasurementDefinitionRepository
{
    private readonly SmartFarmDbContext _context;
    public MeasurementDefinitionRepository(SmartFarmDbContext context) => _context = context;

    public async Task<M.MeasurementDefinition?> GetByIdAsync(Guid id) =>
        await _context.MeasurementDefinitions.Include(m => m.Group).FirstOrDefaultAsync(m => m.Id == id);

    public async Task<List<M.MeasurementDefinition>> GetByExperimentAsync(Guid experimentId) =>
        await _context.MeasurementDefinitions.Include(m => m.Group)
            .Where(m => m.ExperimentId == experimentId).ToListAsync();

    public async Task<M.MeasurementDefinition> CreateAsync(M.MeasurementDefinition entity)
    {
        await _context.MeasurementDefinitions.AddAsync(entity); await _context.SaveChangesAsync(); return entity;
    }

    public async Task UpdateAsync(M.MeasurementDefinition entity)
    {
        _context.MeasurementDefinitions.Update(entity); await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var e = await _context.MeasurementDefinitions.FindAsync(id);
        if (e != null) { _context.MeasurementDefinitions.Remove(e); await _context.SaveChangesAsync(); }
    }
}
