using Microsoft.EntityFrameworkCore;
using M = SmartFarmSEP490.Model;
using SmartFarmSEP490.Repository.DbContexts;
using SmartFarmSEP490.Repository.Interfaces.ExperimentBedAssignments;
using Task = System.Threading.Tasks.Task;

namespace SmartFarmSEP490.Repository.Implementations.ExperimentBedAssignments;

public class ExperimentBedAssignmentRepository : IExperimentBedAssignmentRepository
{
    private readonly SmartFarmDbContext _context;
    public ExperimentBedAssignmentRepository(SmartFarmDbContext context) => _context = context;

    public async Task<M.ExperimentBedAssignment?> GetByIdAsync(Guid id) =>
        await _context.ExperimentBedAssignments
            .Include(e => e.Bed).ThenInclude(b => b.Area).ThenInclude(a => a.Farm)
            .Include(e => e.Experiment).FirstOrDefaultAsync(e => e.Id == id);

    public async Task<List<M.ExperimentBedAssignment>> GetByExperimentAsync(Guid experimentId) =>
        await _context.ExperimentBedAssignments.Include(e => e.Bed).ThenInclude(b => b.Area)
            .Where(e => e.ExperimentId == experimentId).ToListAsync();

    public async Task<M.ExperimentBedAssignment?> GetActiveByBedAsync(Guid bedId) =>
        await _context.ExperimentBedAssignments.Include(e => e.Experiment)
            .Where(e => e.BedId == bedId && e.AssignedTo == null).FirstOrDefaultAsync();

    public async Task<List<M.ExperimentBedAssignment>> GetByBedAsync(Guid bedId) =>
        await _context.ExperimentBedAssignments
            .Include(e => e.Bed).ThenInclude(b => b.Area).ThenInclude(a => a.Farm)
            .Include(e => e.Experiment)
            .Where(e => e.BedId == bedId).ToListAsync();

    public async Task<M.ExperimentBedAssignment> CreateAsync(M.ExperimentBedAssignment entity)
    {
        await _context.ExperimentBedAssignments.AddAsync(entity); await _context.SaveChangesAsync(); return entity;
    }

    public async Task UpdateAsync(M.ExperimentBedAssignment entity)
    {
        _context.ExperimentBedAssignments.Update(entity); await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var e = await _context.ExperimentBedAssignments.FindAsync(id);
        if (e != null) { _context.ExperimentBedAssignments.Remove(e); await _context.SaveChangesAsync(); }
    }
}
