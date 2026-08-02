using Microsoft.EntityFrameworkCore;
using M = SmartFarmSEP490.Model;
using SmartFarmSEP490.Repository.DbContexts;
using SmartFarmSEP490.Repository.Interfaces.ExperimentGroups;
using Task = System.Threading.Tasks.Task;

namespace SmartFarmSEP490.Repository.Implementations.ExperimentGroups;

public class ExperimentGroupRepository : IExperimentGroupRepository
{
    private readonly SmartFarmDbContext _context;
    public ExperimentGroupRepository(SmartFarmDbContext context) => _context = context;

    public async Task<M.ExperimentGroup?> GetByIdAsync(Guid id) => await _context.ExperimentGroups.FindAsync(id);

    public async Task<List<M.ExperimentGroup>> GetByExperimentAsync(Guid experimentId) =>
        await _context.ExperimentGroups.Where(g => g.ExperimentId == experimentId).OrderBy(g => g.GroupName).ToListAsync();

    public async Task<M.ExperimentGroup> CreateAsync(M.ExperimentGroup entity)
    {
        entity.CreatedAt = DateTime.UtcNow;
        await _context.ExperimentGroups.AddAsync(entity); await _context.SaveChangesAsync(); return entity;
    }

    public async Task AddRangeAsync(IEnumerable<M.ExperimentGroup> entities)
    {
        var now = DateTime.UtcNow;
        foreach (var e in entities) e.CreatedAt = now;
        await _context.ExperimentGroups.AddRangeAsync(entities);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(M.ExperimentGroup entity)
    {
        _context.ExperimentGroups.Update(entity); await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var e = await _context.ExperimentGroups.FindAsync(id);
        if (e != null) { _context.ExperimentGroups.Remove(e); await _context.SaveChangesAsync(); }
    }
}
