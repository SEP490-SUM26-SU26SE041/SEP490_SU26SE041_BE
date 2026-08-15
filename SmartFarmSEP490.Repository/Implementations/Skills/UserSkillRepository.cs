using Microsoft.EntityFrameworkCore;
using M = SmartFarmSEP490.Model;
using SmartFarmSEP490.Repository.DbContexts;
using SmartFarmSEP490.Repository.Interfaces.Skills;
using Task = System.Threading.Tasks.Task;

namespace SmartFarmSEP490.Repository.Implementations.Skills;

public class UserSkillRepository : IUserSkillRepository
{
    private readonly SmartFarmDbContext _context;
    public UserSkillRepository(SmartFarmDbContext context) => _context = context;

    public async Task<(Guid UserId, Guid SkillId)?> GetKeyAsync(Guid userId, Guid skillId) =>
        await _context.UserSkills
            .Where(us => us.UserId == userId && us.SkillId == skillId)
            .Select(us => new ValueTuple<Guid, Guid>(us.UserId, us.SkillId))
            .FirstOrDefaultAsync();

    public async Task<M.UserSkill?> GetByKeyAsync(Guid userId, Guid skillId) =>
        await _context.UserSkills
            .Include(us => us.User)
            .Include(us => us.Skill)
            .FirstOrDefaultAsync(us => us.UserId == userId && us.SkillId == skillId);

    public async Task<List<M.UserSkill>> GetAllAsync() =>
        await _context.UserSkills
            .Include(us => us.User)
            .Include(us => us.Skill)
            .OrderBy(us => us.User.FullName).ThenBy(us => us.Skill.SkillName)
            .ToListAsync();

    public async Task<List<M.UserSkill>> GetByUserAsync(Guid userId) =>
        await _context.UserSkills
            .Include(us => us.Skill)
            .Where(us => us.UserId == userId)
            .OrderByDescending(us => us.ProficiencyLevel).ThenBy(us => us.Skill.SkillName)
            .ToListAsync();

    public async Task<List<M.UserSkill>> GetBySkillAsync(Guid skillId) =>
        await _context.UserSkills
            .Include(us => us.User)
            .Where(us => us.SkillId == skillId)
            .OrderByDescending(us => us.ProficiencyLevel).ThenBy(us => us.User.FullName)
            .ToListAsync();

    public async Task<M.UserSkill> CreateAsync(M.UserSkill entity)
    {
        entity.CreatedAt = DateTime.UtcNow;
        await _context.UserSkills.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(M.UserSkill entity)
    {
        _context.UserSkills.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid userId, Guid skillId)
    {
        var entity = await _context.UserSkills
            .FirstOrDefaultAsync(us => us.UserId == userId && us.SkillId == skillId);
        if (entity != null)
        {
            _context.UserSkills.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}