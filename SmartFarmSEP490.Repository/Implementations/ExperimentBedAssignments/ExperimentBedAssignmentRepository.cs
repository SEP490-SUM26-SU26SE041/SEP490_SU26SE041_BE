using Microsoft.EntityFrameworkCore;
using M = SmartFarmSEP490.Model;
using SmartFarmSEP490.Model.Enums;
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
            .Include(e => e.Experiment)
            .FirstOrDefaultAsync(e => e.Id == id);

    public async Task<List<M.ExperimentBedAssignment>> GetByExperimentAsync(Guid experimentId) =>
        await _context.ExperimentBedAssignments
            .Include(e => e.Bed).ThenInclude(b => b.Area)
            .Where(e => e.ExperimentId == experimentId).ToListAsync();

    public async Task<M.ExperimentBedAssignment?> GetActiveByBedAsync(Guid bedId, Guid? currentRequestId = null)
    {
        var query = _context.ExperimentBedAssignments
            .Include(e => e.Experiment)
            .Where(e => e.BedId == bedId && e.AssignedTo == null
                && e.Status.ToString() != "Released");

        if (currentRequestId.HasValue && currentRequestId.Value != Guid.Empty)
        {
            return await query
                .Where(e => e.RequestId != currentRequestId.Value && e.ExperimentId == null)
                .FirstOrDefaultAsync();
        }

        return await query.FirstOrDefaultAsync();
    }

    public async Task<List<M.ExperimentBedAssignment>> GetByBedAsync(Guid bedId) =>
        await _context.ExperimentBedAssignments
            .Include(e => e.Bed).ThenInclude(b => b.Area).ThenInclude(a => a.Farm)
            .Include(e => e.Experiment)
            .Where(e => e.BedId == bedId).ToListAsync();

    public async Task<M.ExperimentBedAssignment> CreateAsync(M.ExperimentBedAssignment entity)
    {
        await _context.ExperimentBedAssignments.AddAsync(entity); await _context.SaveChangesAsync(); return entity;
    }

    public async Task CreateRangeAsync(List<M.ExperimentBedAssignment> entities)
    {
        await _context.ExperimentBedAssignments.AddRangeAsync(entities); await _context.SaveChangesAsync();
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

    public async Task<List<M.ExperimentBedAssignment>> GetByRequestAsync(Guid requestId) =>
        await _context.ExperimentBedAssignments
            .Include(e => e.Bed).ThenInclude(b => b.Area)
            .Where(e => e.RequestId == requestId).ToListAsync();

    public async Task AssignBedsToExperimentAsync(Guid requestId, Guid experimentId)
    {
        var assignments = await _context.ExperimentBedAssignments
            .Where(e => e.RequestId == requestId && e.Status.ToString() == "Reserved").ToListAsync();
        foreach (var a in assignments)
        {
            a.ExperimentId = experimentId;
            a.Status = AllocationStatus.Assigned;
        }
        await _context.SaveChangesAsync();
    }

    public async Task ReleaseBedsAsync(Guid experimentId)
    {
        var assignments = await _context.ExperimentBedAssignments
            .Where(e => e.ExperimentId == experimentId).ToListAsync();
        foreach (var a in assignments)
        {
            a.ExperimentId = null;
            a.Status = AllocationStatus.Released;
            a.AssignedTo = DateOnly.FromDateTime(DateTime.UtcNow);
        }
        await _context.SaveChangesAsync();
    }

    public async Task<List<Guid>> GetAvailableBedIdsByFarmAsync(Guid farmId)
    {
        var farmBedIds = await _context.Beds
            .Where(b => b.Area.FarmId == farmId && b.DeletedAt == null)
            .Select(b => b.Id).ToListAsync();

        var occupiedBedIds = await _context.ExperimentBedAssignments
            .Where(e => farmBedIds.Contains(e.BedId) && e.Status.ToString() != "Released")
            .Select(e => e.BedId).Distinct().ToListAsync();

        return farmBedIds.Except(occupiedBedIds).ToList();
    }

    public async Task UpdateOrCreateAssignmentAsync(Guid requestId, Guid bedId, Guid? experimentId, DateOnly assignedFrom, string? purpose)
    {
        var existing = await _context.ExperimentBedAssignments
            .Where(e => e.RequestId == requestId && e.BedId == bedId)
            .ToListAsync();

        var active = existing.FirstOrDefault(e => e.AssignedTo == null && e.Status.ToString() != "Released");

        if (active != null)
        {
            active.ExperimentId = experimentId;
            active.Status = AllocationStatus.Assigned;
            active.AssignedFrom = assignedFrom;
            active.Purpose = purpose;
        }
        else
        {
            var entity = new M.ExperimentBedAssignment
            {
                RequestId = requestId,
                ExperimentId = experimentId,
                BedId = bedId,
                Status = AllocationStatus.Assigned,
                AssignedFrom = assignedFrom,
                Purpose = purpose
            };
            await _context.ExperimentBedAssignments.AddAsync(entity);
        }
        await _context.SaveChangesAsync();
    }
}
