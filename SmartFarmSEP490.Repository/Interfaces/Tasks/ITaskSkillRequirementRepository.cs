using SmartFarmSEP490.Model;

namespace SmartFarmSEP490.Repository.Interfaces.Tasks;

public interface ITaskSkillRequirementRepository
{
    System.Threading.Tasks.Task<System.Collections.Generic.List<TaskSkillRequirement>> GetByTaskAsync(Guid taskId);
    System.Threading.Tasks.Task AddAsync(TaskSkillRequirement requirement);
    System.Threading.Tasks.Task DeleteByTaskAsync(Guid taskId);
    System.Threading.Tasks.Task<System.Collections.Generic.List<User>> GetUsersByRolesAsync(System.Collections.Generic.List<string> roleNames);
    System.Threading.Tasks.Task<System.Collections.Generic.List<User>> GetUsersWithSkillsAsync(System.Collections.Generic.List<string> roleNames, System.Collections.Generic.List<Guid>? skillIds);
}
