using Microsoft.EntityFrameworkCore;
using M = SmartFarmSEP490.Model;
using SmartFarmSEP490.Model.DTOs;
using SmartFarmSEP490.Repository.DbContexts;
using SmartFarmSEP490.Repository.Interfaces.Farms;
using Task = System.Threading.Tasks.Task;

namespace SmartFarmSEP490.Repository.Implementations.Farms;

public class FarmRepository : IFarmRepository
{
    private readonly SmartFarmDbContext _context;
    public FarmRepository(SmartFarmDbContext context) => _context = context;

    public async Task<M.Farm?> GetByIdAsync(Guid id) => await _context.Farms.FindAsync(id);

    public async Task<M.Farm?> GetByIdWithDetailsAsync(Guid id) =>
        await _context.Farms
            .Include(f => f.Manager)
            .Include(f => f.Areas).ThenInclude(a => a.Beds).ThenInclude(b => b.ExperimentBedAssignments)
            .FirstOrDefaultAsync(f => f.Id == id);

    public async Task<List<M.Farm>> GetAllAsync() =>
        await _context.Farms
            .Include(f => f.Manager)
            .Include(f => f.Areas).ThenInclude(a => a.Beds).ThenInclude(b => b.ExperimentBedAssignments)
            .OrderBy(f => f.FarmName)
            .ToListAsync();

    public async Task<M.Farm?> GetByCodeAsync(string code) =>
        await _context.Farms.FirstOrDefaultAsync(f => f.FarmCode == code);

    public async Task<List<M.Farm>> GetByManagerAsync(Guid managerId) =>
        await _context.Farms
            .Include(f => f.Manager)
            .Include(f => f.Areas).ThenInclude(a => a.Beds).ThenInclude(b => b.ExperimentBedAssignments)
            .Where(f => f.ManagerId == managerId)
            .OrderBy(f => f.FarmName)
            .ToListAsync();

    public async Task<M.Farm> CreateAsync(M.Farm entity)
    {
        entity.CreatedAt = DateTime.UtcNow; entity.UpdatedAt = DateTime.UtcNow;
        await _context.Farms.AddAsync(entity); await _context.SaveChangesAsync(); return entity;
    }

    public async Task UpdateAsync(M.Farm entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _context.Farms.Update(entity); await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var e = await _context.Farms.FindAsync(id);
        if (e != null) { e.DeletedAt = DateTime.UtcNow; await UpdateAsync(e); }
    }

    public async Task<FarmResourceSummaryDto?> GetFarmResourceSummaryAsync(Guid farmId)
    {
        var farm = await _context.Farms.FindAsync(farmId);
        if (farm == null) return null;

        var beds = await _context.Beds
            .Include(b => b.Area)
            .Include(b => b.ExperimentBedAssignments)
            .Where(b => b.Area.FarmId == farmId && b.DeletedAt == null)
            .ToListAsync();

        var totalSensors = await _context.Sensors.CountAsync();

        var inUseAssignments = beds
            .SelectMany(b => b.ExperimentBedAssignments)
            .Where(a => a.AssignedTo == null)
            .Select(a => a.BedId)
            .Distinct()
            .Count();

        return new FarmResourceSummaryDto
        {
            FarmId = farmId,
            FarmName = farm.FarmName,
            TotalBeds = beds.Count,
            AvailableBeds = beds.Count - inUseAssignments,
            InUseBeds = inUseAssignments,
            MaintenanceBeds = 0,
            TotalSensors = totalSensors,
            TotalAreas = beds.Select(b => b.AreaId).Distinct().Count()
        };
    }
}
