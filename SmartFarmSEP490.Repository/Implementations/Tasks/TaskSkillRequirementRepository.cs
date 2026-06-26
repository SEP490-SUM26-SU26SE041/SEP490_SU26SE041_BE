using Microsoft.EntityFrameworkCore;
using SmartFarmSEP490.Model;
using SmartFarmSEP490.Repository.DbContexts;
using SmartFarmSEP490.Repository.Interfaces.Tasks;
using Task = System.Threading.Tasks.Task;

namespace SmartFarmSEP490.Repository.Implementations.Tasks;

public class TaskSkillRequirementRepository : ITaskSkillRequirementRepository
{
    private readonly SmartFarmDbContext _context;

    public TaskSkillRequirementRepository(SmartFarmDbContext context)
    {
        _context = context;
    }

    public async Task<List<TaskSkillRequirement>> GetByTaskAsync(Guid taskId)
    {
        return await _context.TaskSkillRequirements
            .Include(tsr => tsr.Skill)
            .Where(tsr => tsr.TaskId == taskId)
            .ToListAsync();
    }

    public async Task AddAsync(TaskSkillRequirement requirement)
    {
        await _context.TaskSkillRequirements.AddAsync(requirement);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteByTaskAsync(Guid taskId)
    {
        var requirements = await _context.TaskSkillRequirements
            .Where(tsr => tsr.TaskId == taskId)
            .ToListAsync();
        _context.TaskSkillRequirements.RemoveRange(requirements);
        await _context.SaveChangesAsync();
    }

    public async Task<List<User>> GetUsersByRolesAsync(List<string> roleNames)
    {
        return await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .Include(u => u.UserSkills)
                .ThenInclude(us => us.Skill)
            .Where(u => u.IsActive && u.UserRoles.Any(ur => roleNames.Contains(ur.Role.RoleName)))
            .ToListAsync();
    }

    public async Task<List<User>> GetUsersWithSkillsAsync(List<string> roleNames, List<Guid>? skillIds)
    {
        var query = _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .Include(u => u.UserSkills)
                .ThenInclude(us => us.Skill)
            .Where(u => u.IsActive && u.UserRoles.Any(ur => roleNames.Contains(ur.Role.RoleName)));

        return await query.ToListAsync();
    }
}
