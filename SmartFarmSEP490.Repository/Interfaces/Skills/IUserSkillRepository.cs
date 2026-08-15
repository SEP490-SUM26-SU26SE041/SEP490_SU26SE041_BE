using M = SmartFarmSEP490.Model;
using Task = System.Threading.Tasks.Task;

namespace SmartFarmSEP490.Repository.Interfaces.Skills;

public interface IUserSkillRepository
{
    Task<(Guid UserId, Guid SkillId)?> GetKeyAsync(Guid userId, Guid skillId);
    Task<M.UserSkill?> GetByKeyAsync(Guid userId, Guid skillId);
    Task<List<M.UserSkill>> GetAllAsync();
    Task<List<M.UserSkill>> GetByUserAsync(Guid userId);
    Task<List<M.UserSkill>> GetBySkillAsync(Guid skillId);
    Task<M.UserSkill> CreateAsync(M.UserSkill entity);
    Task UpdateAsync(M.UserSkill entity);
    Task DeleteAsync(Guid userId, Guid skillId);
}