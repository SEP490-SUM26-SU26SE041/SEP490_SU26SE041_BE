using Microsoft.EntityFrameworkCore;
using M = SmartFarmSEP490.Model;
using SmartFarmSEP490.Repository.DbContexts;
using SmartFarmSEP490.Repository.Interfaces.ProcedureTemplates;
using Task = System.Threading.Tasks.Task;

namespace SmartFarmSEP490.Repository.Implementations.ProcedureTemplates;

public class ProcedureTemplateRepository : IProcedureTemplateRepository
{
    private readonly SmartFarmDbContext _context;
    public ProcedureTemplateRepository(SmartFarmDbContext context) => _context = context;

    public async Task<M.ProcedureTemplate?> GetByIdAsync(Guid id) => await _context.ProcedureTemplates.FindAsync(id);

    public async Task<M.ProcedureTemplate?> GetByIdWithStepsAsync(Guid id) =>
        await _context.ProcedureTemplates
            .Include(p => p.CropVariety)
            .Include(p => p.ProcedureTemplateSteps.OrderBy(s => s.StepOrder))
            .FirstOrDefaultAsync(p => p.Id == id);

    public async Task<List<M.ProcedureTemplate>> GetAllAsync() =>
        await _context.ProcedureTemplates.Include(p => p.CropVariety).OrderBy(p => p.TemplateName).ToListAsync();

    public async Task<List<M.ProcedureTemplate>> GetByCropVarietyAsync(Guid cropVarietyId) =>
        await _context.ProcedureTemplates
            .Where(p => p.CropVarietyId == cropVarietyId)
            .Include(p => p.ProcedureTemplateSteps.OrderBy(s => s.StepOrder)).ToListAsync();

    public async Task<M.ProcedureTemplate> CreateAsync(M.ProcedureTemplate entity)
    {
        entity.CreatedAt = DateTime.UtcNow;
        foreach (var step in entity.ProcedureTemplateSteps)
            step.TemplateId = entity.Id;
        await _context.ProcedureTemplates.AddAsync(entity);
        await _context.ProcedureTemplateSteps.AddRangeAsync(entity.ProcedureTemplateSteps);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(M.ProcedureTemplate entity)
    {
        _context.ProcedureTemplates.Update(entity); await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var e = await _context.ProcedureTemplates.FindAsync(id);
        if (e != null) { _context.ProcedureTemplates.Remove(e); await _context.SaveChangesAsync(); }
    }
}
