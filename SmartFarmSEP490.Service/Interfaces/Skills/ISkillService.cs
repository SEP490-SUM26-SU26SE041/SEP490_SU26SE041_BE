using SmartFarmSEP490.Model.DTOs;

namespace SmartFarmSEP490.Service.Interfaces.Skills;

public interface ISkillService
{
    Task<SkillResponseDto?> CreateAsync(CreateSkillDto dto);
    Task<SkillResponseDto?> UpdateAsync(Guid id, UpdateSkillDto dto);
    Task<SkillResponseDto?> GetByIdAsync(Guid id);
    Task<List<SkillResponseDto>> GetAllAsync();
    Task<bool> DeleteAsync(Guid id);
}

public interface IUserSkillService
{
    Task<UserSkillResponseDto?> CreateAsync(CreateUserSkillDto dto);
    Task<UserSkillResponseDto?> UpdateAsync(Guid userId, Guid skillId, UpdateUserSkillDto dto);
    Task<UserSkillResponseDto?> GetByKeyAsync(Guid userId, Guid skillId);
    Task<List<UserSkillResponseDto>> GetAllAsync();
    Task<List<UserSkillResponseDto>> GetByUserAsync(Guid userId);
    Task<List<UserSkillResponseDto>> GetBySkillAsync(Guid skillId);
    Task<bool> DeleteAsync(Guid userId, Guid skillId);
}

public interface ITaskCountService
{
    Task<DailyTaskCountReportDto> GetDailyCountByRoleAsync(
        IReadOnlyCollection<string> roleNames,
        DateOnly date,
        CancellationToken ct = default);
}