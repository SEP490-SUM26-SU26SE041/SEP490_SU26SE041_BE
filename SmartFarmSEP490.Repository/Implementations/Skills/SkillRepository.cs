using Microsoft.EntityFrameworkCore;
using M = SmartFarmSEP490.Model;
using SmartFarmSEP490.Repository.DbContexts;
using SmartFarmSEP490.Repository.Interfaces.Skills;
using Task = System.Threading.Tasks.Task;

namespace SmartFarmSEP490.Repository.Implementations.Skills;

public class SkillRepository : ISkillRepository
{
    private readonly SmartFarmDbContext _context;
    public SkillRepository(SmartFarmDbContext context) => _context = context;

    public async Task<M.Skill?> GetByIdAsync(Guid id) =>
        await _context.Skills.FindAsync(id);

    public async Task<M.Skill?> GetByNameAsync(string skillName) =>
        await _context.Skills
            .FirstOrDefaultAsync(s => s.SkillName.ToLower() == skillName.ToLower());

    public async Task<List<M.Skill>> GetAllAsync() =>
        await _context.Skills
            .OrderBy(s => s.SkillName)
            .ToListAsync();

    public async Task<M.Skill> CreateAsync(M.Skill entity)
    {
        entity.CreatedAt = DateTime.UtcNow;
        await _context.Skills.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(M.Skill entity)
    {
        _context.Skills.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var skill = await _context.Skills.FindAsync(id);
        if (skill == null) return;

        var hasUsers = await _context.UserSkills.AnyAsync(us => us.SkillId == id);
        var hasTasks = await _context.TaskSkillRequirements.AnyAsync(tsr => tsr.SkillId == id);
        if (hasUsers || hasTasks)
        {
            throw new InvalidOperationException(
                "Khong the xoa Skill vi dang duoc su dung boi UserSkills hoac TaskSkillRequirements.");
        }

        _context.Skills.Remove(skill);
        await _context.SaveChangesAsync();
    }
}