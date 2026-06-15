using Microsoft.EntityFrameworkCore;
using M = SmartFarmSEP490.Model;
using SmartFarmSEP490.Repository.DbContexts;
using SmartFarmSEP490.Repository.Interfaces.ExperimentRequests;
using Task = System.Threading.Tasks.Task;

namespace SmartFarmSEP490.Repository.Implementations.ExperimentRequests;

public class ExperimentRequestRepository : IExperimentRequestRepository
{
    private readonly SmartFarmDbContext _context;
    public ExperimentRequestRepository(SmartFarmDbContext context) => _context = context;

    public async Task<M.ExperimentRequest?> GetByIdAsync(Guid id) => await _context.ExperimentRequests.FindAsync(id);

    public async Task<M.ExperimentRequest?> GetByIdWithDetailsAsync(Guid id) =>
        await _context.ExperimentRequests
            .Include(r => r.Researcher).Include(r => r.Farm)
            .Include(r => r.CropVariety).Include(r => r.ProcedureTemplate)
            .Include(r => r.RequestReviews).ThenInclude(rr => rr.Reviewer)
            .FirstOrDefaultAsync(r => r.Id == id);

    public async Task<List<M.ExperimentRequest>> GetAllAsync() =>
        await _context.ExperimentRequests.Include(r => r.Researcher).Include(r => r.Farm)
            .OrderByDescending(r => r.CreatedAt).ToListAsync();

    public async Task<List<M.ExperimentRequest>> GetByResearcherAsync(Guid researcherId) =>
        await _context.ExperimentRequests.Include(r => r.Farm)
            .Where(r => r.ResearcherId == researcherId).OrderByDescending(r => r.CreatedAt).ToListAsync();

    public async Task<List<M.ExperimentRequest>> GetByFarmAsync(Guid farmId) =>
        await _context.ExperimentRequests.Include(r => r.Researcher)
            .Where(r => r.FarmId == farmId).OrderByDescending(r => r.CreatedAt).ToListAsync();

    public async Task<List<M.ExperimentRequest>> GetByStatusAsync(string status) =>
        await _context.ExperimentRequests.Include(r => r.Researcher).Include(r => r.Farm)
            .Where(r => r.Status == status).OrderByDescending(r => r.CreatedAt).ToListAsync();

    public async Task<M.ExperimentRequest> CreateAsync(M.ExperimentRequest entity)
    {
        entity.CreatedAt = DateTime.UtcNow; entity.UpdatedAt = DateTime.UtcNow;
        await _context.ExperimentRequests.AddAsync(entity); await _context.SaveChangesAsync(); return entity;
    }

    public async Task UpdateAsync(M.ExperimentRequest entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _context.ExperimentRequests.Update(entity); await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var e = await _context.ExperimentRequests.FindAsync(id);
        if (e != null) { _context.ExperimentRequests.Remove(e); await _context.SaveChangesAsync(); }
    }
}
