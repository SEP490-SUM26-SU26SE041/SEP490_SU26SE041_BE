using M = SmartFarmSEP490.Model;
using Task = System.Threading.Tasks.Task;

namespace SmartFarmSEP490.Repository.Interfaces.Skills;

public interface ISkillRepository
{
    Task<M.Skill?> GetByIdAsync(Guid id);
    Task<M.Skill?> GetByNameAsync(string skillName);
    Task<List<M.Skill>> GetAllAsync();
    Task<M.Skill> CreateAsync(M.Skill entity);
    Task UpdateAsync(M.Skill entity);
    Task DeleteAsync(Guid id);
}