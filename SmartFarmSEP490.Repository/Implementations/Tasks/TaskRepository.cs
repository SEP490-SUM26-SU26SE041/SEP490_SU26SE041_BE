using Microsoft.EntityFrameworkCore;
using SmartFarmSEP490.Model;
using SmartFarmSEP490.Repository.DbContexts;
using SmartFarmSEP490.Repository.Interfaces.Tasks;

namespace SmartFarmSEP490.Repository.Implementations.Tasks;

public class TaskRepository : ITaskRepository
{
    private readonly SmartFarmDbContext _context;

    public TaskRepository(SmartFarmDbContext context)
    {
        _context = context;
    }

    public async System.Threading.Tasks.Task<Model.Task?> GetByIdAsync(Guid id)
    {
        return await _context.Tasks
            .Include(t => t.Experiment)
            .Include(t => t.ExperimentStage)
            .Include(t => t.Batch)
            .Include(t => t.CareSchedule)
            .Include(t => t.CreatedByNavigation)
            .Include(t => t.AssignedToNavigation)
            .Include(t => t.TaskSkillRequirements)
                .ThenInclude(tsr => tsr.Skill)
            .Include(t => t.TaskAssignments)
                .ThenInclude(ta => ta.Assignee)
                    .ThenInclude(a => a.UserRoles)
                        .ThenInclude(ur => ur.Role)
            .Include(t => t.TaskAssignments)
                .ThenInclude(ta => ta.AssignedByNavigation)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async System.Threading.Tasks.Task<List<Model.Task>> GetByExperimentAsync(Guid experimentId)
    {
        return await _context.Tasks
            .Include(t => t.Experiment)
            .Include(t => t.ExperimentStage)
            .Include(t => t.Batch)
            .Include(t => t.CreatedByNavigation)
            .Include(t => t.AssignedToNavigation)
            .Include(t => t.TaskSkillRequirements)
                .ThenInclude(tsr => tsr.Skill)
            .Where(t => t.ExperimentId == experimentId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async System.Threading.Tasks.Task<List<Model.Task>> GetByAssigneeAsync(Guid assigneeId)
    {
        return await _context.Tasks
            .Include(t => t.Experiment)
            .Include(t => t.ExperimentStage)
            .Include(t => t.Batch)
            .Include(t => t.CreatedByNavigation)
            .Include(t => t.AssignedToNavigation)
            .Include(t => t.TaskSkillRequirements)
                .ThenInclude(tsr => tsr.Skill)
            .Where(t => t.AssignedTo == assigneeId)
            .OrderByDescending(t => t.DueDate)
            .ToListAsync();
    }

    public async System.Threading.Tasks.Task<List<Model.Task>> GetAllAsync()
    {
        return await _context.Tasks
            .Include(t => t.Experiment)
            .Include(t => t.ExperimentStage)
            .Include(t => t.Batch)
            .Include(t => t.CreatedByNavigation)
            .Include(t => t.AssignedToNavigation)
            .Include(t => t.TaskSkillRequirements)
                .ThenInclude(tsr => tsr.Skill)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async System.Threading.Tasks.Task<Model.Task> AddAsync(Model.Task task)
    {
        await _context.Tasks.AddAsync(task);
        await _context.SaveChangesAsync();
        return task;
    }

    public async System.Threading.Tasks.Task UpdateAsync(Model.Task task)
    {
        _context.Tasks.Update(task);
        await _context.SaveChangesAsync();
    }

    public async System.Threading.Tasks.Task DeleteAsync(Guid id)
    {
        var task = await _context.Tasks.FindAsync(id);
        if (task != null)
        {
            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();
        }
    }
}
