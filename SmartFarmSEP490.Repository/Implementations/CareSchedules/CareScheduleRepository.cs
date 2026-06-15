using Microsoft.EntityFrameworkCore;
using M = SmartFarmSEP490.Model;
using SmartFarmSEP490.Repository.DbContexts;
using SmartFarmSEP490.Repository.Interfaces.CareSchedules;
using Task = System.Threading.Tasks.Task;

namespace SmartFarmSEP490.Repository.Implementations.CareSchedules;

public class CareScheduleRepository : ICareScheduleRepository
{
    private readonly SmartFarmDbContext _context;
    public CareScheduleRepository(SmartFarmDbContext context) => _context = context;

    public async Task<M.CareSchedule?> GetByIdAsync(Guid id) =>
        await _context.CareSchedules.Include(c => c.ExperimentStage).Include(c => c.Batch)
            .FirstOrDefaultAsync(c => c.Id == id);

    public async Task<List<M.CareSchedule>> GetByExperimentAsync(Guid experimentId) =>
        await _context.CareSchedules.Include(c => c.ExperimentStage).Include(c => c.Batch)
            .Where(c => c.ExperimentId == experimentId).OrderBy(c => c.StartDate).ToListAsync();

    public async Task<M.CareSchedule> CreateAsync(M.CareSchedule entity)
    {
        entity.CreatedAt = DateTime.UtcNow;
        await _context.CareSchedules.AddAsync(entity); await _context.SaveChangesAsync(); return entity;
    }

    public async Task UpdateAsync(M.CareSchedule entity)
    {
        _context.CareSchedules.Update(entity); await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var e = await _context.CareSchedules.FindAsync(id);
        if (e != null) { _context.CareSchedules.Remove(e); await _context.SaveChangesAsync(); }
    }
}
