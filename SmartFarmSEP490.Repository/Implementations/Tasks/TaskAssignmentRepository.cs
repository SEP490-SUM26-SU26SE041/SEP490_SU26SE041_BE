using Microsoft.EntityFrameworkCore;
using SmartFarmSEP490.Model;
using SmartFarmSEP490.Repository.DbContexts;
using SmartFarmSEP490.Repository.Interfaces.Tasks;
using Task = System.Threading.Tasks.Task;

namespace SmartFarmSEP490.Repository.Implementations.Tasks;

public class TaskAssignmentRepository : ITaskAssignmentRepository
{
    private readonly SmartFarmDbContext _context;

    public TaskAssignmentRepository(SmartFarmDbContext context)
    {
        _context = context;
    }

    public async Task<TaskAssignment?> GetByIdAsync(Guid id)
    {
        return await _context.TaskAssignments
            .Include(ta => ta.Task)
            .Include(ta => ta.Assignee)
                .ThenInclude(a => a.UserRoles)
                    .ThenInclude(ur => ur.Role)
            .Include(ta => ta.AssignedByNavigation)
            .FirstOrDefaultAsync(ta => ta.Id == id);
    }

    public async Task<List<TaskAssignment>> GetByTaskIdAsync(Guid taskId)
    {
        return await _context.TaskAssignments
            .Include(ta => ta.Assignee)
                .ThenInclude(a => a.UserRoles)
                    .ThenInclude(ur => ur.Role)
            .Include(ta => ta.AssignedByNavigation)
            .Where(ta => ta.TaskId == taskId)
            .OrderByDescending(ta => ta.AssignedAt)
            .ToListAsync();
    }

    public async Task<List<TaskAssignment>> GetByAssigneeAsync(Guid assigneeId)
    {
        return await _context.TaskAssignments
            .Include(ta => ta.Task)
                .ThenInclude(t => t.Experiment)
            .Include(ta => ta.Assignee)
            .Include(ta => ta.AssignedByNavigation)
            .Where(ta => ta.AssigneeId == assigneeId)
            .OrderByDescending(ta => ta.AssignedAt)
            .ToListAsync();
    }

    public async Task<TaskAssignment?> GetActiveByTaskAndAssigneeAsync(Guid taskId, Guid assigneeId)
    {
        return await _context.TaskAssignments
            .Include(ta => ta.Assignee)
            .FirstOrDefaultAsync(ta =>
                ta.TaskId == taskId &&
                ta.AssigneeId == assigneeId &&
                ta.EndedAt == null);
    }

    public async Task<TaskAssignment> AddAsync(TaskAssignment assignment)
    {
        await _context.TaskAssignments.AddAsync(assignment);
        await _context.SaveChangesAsync();
        return assignment;
    }

    public async Task UpdateAsync(TaskAssignment assignment)
    {
        _context.TaskAssignments.Update(assignment);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var assignment = await _context.TaskAssignments.FindAsync(id);
        if (assignment != null)
        {
            _context.TaskAssignments.Remove(assignment);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> HasActiveAssignmentAsync(Guid taskId)
    {
        return await _context.TaskAssignments
            .AnyAsync(ta => ta.TaskId == taskId && ta.EndedAt == null);
    }
}
