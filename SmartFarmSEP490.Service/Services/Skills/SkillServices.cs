using Microsoft.EntityFrameworkCore;
using M = SmartFarmSEP490.Model;
using SmartFarmSEP490.Model.DTOs;
using SmartFarmSEP490.Model.Helpers;
using SmartFarmSEP490.Repository.Interfaces.Skills;
using SmartFarmSEP490.Repository.Interfaces.Tasks;
using SvcInterfaces = SmartFarmSEP490.Service.Interfaces.Skills;

namespace SmartFarmSEP490.Service.Services.Skills;

public class SkillService : SvcInterfaces.ISkillService
{
    private readonly ISkillRepository _skillRepository;
    private readonly SmartFarmSEP490.Repository.DbContexts.SmartFarmDbContext _context;

    public SkillService(
        ISkillRepository skillRepository,
        SmartFarmSEP490.Repository.DbContexts.SmartFarmDbContext context)
    {
        _skillRepository = skillRepository;
        _context = context;
    }

    public async Task<SkillResponseDto?> CreateAsync(CreateSkillDto dto)
    {
        var trimmed = (dto.SkillName ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
            throw new InvalidOperationException("SkillName khong duoc de trong.");

        var existing = await _skillRepository.GetByNameAsync(trimmed);
        if (existing != null)
            throw new InvalidOperationException($"Skill '{trimmed}' da ton tai.");

        try
        {
            var entity = new M.Skill
            {
                SkillName = trimmed,
                Description = dto.Description?.Trim()
            };
            var created = await _skillRepository.CreateAsync(entity);
            return await GetByIdAsync(created.Id);
        }
        catch (DbUpdateException dbEx)
        {
            throw new InvalidOperationException(
                $"Tao Skill that bai: {dbEx.InnerException?.Message ?? dbEx.Message}", dbEx);
        }
    }

    public async Task<SkillResponseDto?> UpdateAsync(Guid id, UpdateSkillDto dto)
    {
        var entity = await _skillRepository.GetByIdAsync(id);
        if (entity == null) return null;

        if (!string.IsNullOrWhiteSpace(dto.SkillName))
        {
            var trimmed = dto.SkillName.Trim();
            if (!string.Equals(trimmed, entity.SkillName, StringComparison.OrdinalIgnoreCase))
            {
                var conflict = await _skillRepository.GetByNameAsync(trimmed);
                if (conflict != null && conflict.Id != id)
                    throw new InvalidOperationException($"Skill '{trimmed}' da ton tai.");
                entity.SkillName = trimmed;
            }
        }

        if (dto.Description != null) entity.Description = dto.Description.Trim();
        await _skillRepository.UpdateAsync(entity);
        return await GetByIdAsync(id);
    }

    public async Task<SkillResponseDto?> GetByIdAsync(Guid id)
    {
        var entity = await _skillRepository.GetByIdAsync(id);
        if (entity == null) return null;
        return await MapToDto(entity);
    }

    public async Task<List<SkillResponseDto>> GetAllAsync()
    {
        var entities = await _skillRepository.GetAllAsync();
        var result = new List<SkillResponseDto>(entities.Count);
        foreach (var e in entities) result.Add(await MapToDto(e));
        return result;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await _skillRepository.GetByIdAsync(id);
        if (entity == null) return false;
        await _skillRepository.DeleteAsync(id);
        return true;
    }

    private async Task<SkillResponseDto> MapToDto(M.Skill e)
    {
        var users = await _context.UserSkills.CountAsync(us => us.SkillId == e.Id);
        var tasks = await _context.TaskSkillRequirements.CountAsync(tsr => tsr.SkillId == e.Id);
        return new SkillResponseDto
        {
            Id = e.Id,
            SkillName = e.SkillName,
            Description = e.Description,
            CreatedAt = e.CreatedAt,
            TotalUsers = users,
            TotalTasks = tasks
        };
    }
}

public class UserSkillService : SvcInterfaces.IUserSkillService
{
    private readonly IUserSkillRepository _userSkillRepository;
    private readonly ISkillRepository _skillRepository;
    private readonly SmartFarmSEP490.Repository.DbContexts.SmartFarmDbContext _context;

    public UserSkillService(
        IUserSkillRepository userSkillRepository,
        ISkillRepository skillRepository,
        SmartFarmSEP490.Repository.DbContexts.SmartFarmDbContext context)
    {
        _userSkillRepository = userSkillRepository;
        _skillRepository = skillRepository;
        _context = context;
    }

    public async Task<UserSkillResponseDto?> CreateAsync(CreateUserSkillDto dto)
    {
        var skill = await _skillRepository.GetByIdAsync(dto.SkillId);
        if (skill == null)
            throw new InvalidOperationException($"SkillId {dto.SkillId} khong ton tai.");

        var existing = await _userSkillRepository.GetKeyAsync(dto.UserId, dto.SkillId);
        if (existing.HasValue)
            throw new InvalidOperationException("User nay da co Skill nay roi (UserId+SkillId la khoa chinh).");

        try
        {
            var entity = new M.UserSkill
            {
                UserId = dto.UserId,
                SkillId = dto.SkillId,
                ProficiencyLevel = dto.ProficiencyLevel,
                Description = dto.Description?.Trim()
            };
            var created = await _userSkillRepository.CreateAsync(entity);
            return await GetByKeyAsync(created.UserId, created.SkillId);
        }
        catch (DbUpdateException dbEx)
        {
            throw new InvalidOperationException(
                $"Tao UserSkill that bai: {dbEx.InnerException?.Message ?? dbEx.Message}", dbEx);
        }
    }

    public async Task<UserSkillResponseDto?> UpdateAsync(Guid userId, Guid skillId, UpdateUserSkillDto dto)
    {
        var entity = await _userSkillRepository.GetByKeyAsync(userId, skillId);
        if (entity == null) return null;
        if (dto.ProficiencyLevel.HasValue) entity.ProficiencyLevel = dto.ProficiencyLevel.Value;
        if (dto.Description != null) entity.Description = dto.Description.Trim();
        await _userSkillRepository.UpdateAsync(entity);
        return await GetByKeyAsync(userId, skillId);
    }

    public async Task<UserSkillResponseDto?> GetByKeyAsync(Guid userId, Guid skillId)
    {
        var entity = await _userSkillRepository.GetByKeyAsync(userId, skillId);
        if (entity == null) return null;

        var roleName = await _context.UserRoles
            .Where(ur => ur.UserId == userId)
            .Select(ur => ur.Role.RoleName)
            .FirstOrDefaultAsync() ?? string.Empty;

        return new UserSkillResponseDto
        {
            UserId = entity.UserId,
            UserName = entity.User?.FullName ?? string.Empty,
            UserEmail = entity.User?.Email ?? string.Empty,
            RoleName = roleName,
            SkillId = entity.SkillId,
            SkillName = entity.Skill?.SkillName ?? string.Empty,
            ProficiencyLevel = entity.ProficiencyLevel,
            Description = entity.Description,
            CreatedAt = entity.CreatedAt
        };
    }

    public async Task<List<UserSkillResponseDto>> GetAllAsync()
    {
        var entities = await _userSkillRepository.GetAllAsync();
        if (entities.Count == 0) return new List<UserSkillResponseDto>();

        var userIds = entities.Select(e => e.UserId).Distinct().ToList();
        var roleMap = await _context.UserRoles
            .Where(ur => userIds.Contains(ur.UserId))
            .GroupBy(ur => ur.UserId)
            .Select(g => new { UserId = g.Key, RoleName = g.Select(x => x.Role.RoleName).FirstOrDefault() })
            .ToDictionaryAsync(x => x.UserId, x => x.RoleName ?? string.Empty);

        return entities.Select(us => new UserSkillResponseDto
        {
            UserId = us.UserId,
            UserName = us.User?.FullName ?? string.Empty,
            UserEmail = us.User?.Email ?? string.Empty,
            RoleName = roleMap.TryGetValue(us.UserId, out var r) ? r : string.Empty,
            SkillId = us.SkillId,
            SkillName = us.Skill?.SkillName ?? string.Empty,
            ProficiencyLevel = us.ProficiencyLevel,
            Description = us.Description,
            CreatedAt = us.CreatedAt
        }).ToList();
    }

    public async Task<List<UserSkillResponseDto>> GetByUserAsync(Guid userId)
    {
        var entities = await _userSkillRepository.GetByUserAsync(userId);
        if (entities.Count == 0) return new List<UserSkillResponseDto>();

        var roleName = await _context.UserRoles
            .Where(ur => ur.UserId == userId)
            .Select(ur => ur.Role.RoleName)
            .FirstOrDefaultAsync() ?? string.Empty;

        return entities.Select(us => new UserSkillResponseDto
        {
            UserId = us.UserId,
            UserName = us.User?.FullName ?? string.Empty,
            UserEmail = us.User?.Email ?? string.Empty,
            RoleName = roleName,
            SkillId = us.SkillId,
            SkillName = us.Skill?.SkillName ?? string.Empty,
            ProficiencyLevel = us.ProficiencyLevel,
            Description = us.Description,
            CreatedAt = us.CreatedAt
        }).ToList();
    }

    public async Task<List<UserSkillResponseDto>> GetBySkillAsync(Guid skillId)
    {
        var entities = await _userSkillRepository.GetBySkillAsync(skillId);
        if (entities.Count == 0) return new List<UserSkillResponseDto>();

        var userIds = entities.Select(e => e.UserId).Distinct().ToList();
        var roleMap = await _context.UserRoles
            .Where(ur => userIds.Contains(ur.UserId))
            .GroupBy(ur => ur.UserId)
            .Select(g => new { UserId = g.Key, RoleName = g.Select(x => x.Role.RoleName).FirstOrDefault() })
            .ToDictionaryAsync(x => x.UserId, x => x.RoleName ?? string.Empty);

        return entities.Select(us => new UserSkillResponseDto
        {
            UserId = us.UserId,
            UserName = us.User?.FullName ?? string.Empty,
            UserEmail = us.User?.Email ?? string.Empty,
            RoleName = roleMap.TryGetValue(us.UserId, out var r) ? r : string.Empty,
            SkillId = us.SkillId,
            SkillName = us.Skill?.SkillName ?? string.Empty,
            ProficiencyLevel = us.ProficiencyLevel,
            Description = us.Description,
            CreatedAt = us.CreatedAt
        }).ToList();
    }

    public async Task<bool> DeleteAsync(Guid userId, Guid skillId)
    {
        var existing = await _userSkillRepository.GetKeyAsync(userId, skillId);
        if (!existing.HasValue) return false;
        await _userSkillRepository.DeleteAsync(userId, skillId);
        return true;
    }
}

public class TaskCountService : SvcInterfaces.ITaskCountService
{
    private readonly ITaskRepository _taskRepository;
    private readonly SmartFarmSEP490.Repository.DbContexts.SmartFarmDbContext _context;
    private static readonly string[] DefaultRoles = { "Technician", "Student" };

    public TaskCountService(
        ITaskRepository taskRepository,
        SmartFarmSEP490.Repository.DbContexts.SmartFarmDbContext context)
    {
        _taskRepository = taskRepository;
        _context = context;
    }

    public async Task<DailyTaskCountReportDto> GetDailyCountByRoleAsync(
        IReadOnlyCollection<string> roleNames,
        DateOnly date,
        CancellationToken ct = default)
    {
        var roles = (roleNames != null && roleNames.Count > 0)
            ? roleNames.Distinct(StringComparer.OrdinalIgnoreCase).ToList()
            : DefaultRoles.ToList();

        // Cửa sổ ICT cho ngày: [00:00 ICT, 00:00 ICT ngày mai)
        var (startUtc, endUtc) = GetIctDayWindowUtc(date);

        var rows = await _taskRepository.CountTasksByUserAsync(roles, startUtc, endUtc, ct);
        if (rows.Count == 0)
        {
            return new DailyTaskCountReportDto
            {
                Date = date,
                StartUtc = startUtc,
                EndUtc = endUtc
            };
        }

        var userIds = rows.Select(r => r.UserId).Distinct().ToList();
        var users = await _context.Users
            .AsNoTracking()
            .Where(u => userIds.Contains(u.Id) && u.DeletedAt == null && u.IsActive)
            .Select(u => new
            {
                u.Id,
                u.FullName,
                u.Email,
                Roles = u.UserRoles.Select(ur => ur.Role.RoleName).ToList()
            })
            .ToListAsync(ct);

        var report = new DailyTaskCountReportDto
        {
            Date = date,
            StartUtc = startUtc,
            EndUtc = endUtc
        };

        foreach (var u in users)
        {
            var primaryRole = u.Roles.FirstOrDefault(r => roles.Contains(r, StringComparer.OrdinalIgnoreCase))
                              ?? u.Roles.FirstOrDefault() ?? string.Empty;

            var userRows = rows.Where(r => r.UserId == u.Id).ToList();
            var dto = new UserTaskCountDto
            {
                UserId = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                RoleName = primaryRole,
                TotalTasks = userRows.Sum(r => r.Count)
            };
            foreach (var r in userRows)
            {
                switch ((Model.Enums.TaskStatus)r.Status)
                {
                    case Model.Enums.TaskStatus.Pending: dto.PendingTasks += r.Count; break;
                    case Model.Enums.TaskStatus.InProgress: dto.InProgressTasks += r.Count; break;
                    case Model.Enums.TaskStatus.Completed: dto.CompletedTasks += r.Count; break;
                    case Model.Enums.TaskStatus.Overdue: dto.OverdueTasks += r.Count; break;
                    case Model.Enums.TaskStatus.Cancelled: dto.CancelledTasks += r.Count; break;
                }
            }
            report.Users.Add(dto);

            if (string.Equals(primaryRole, "Technician", StringComparison.OrdinalIgnoreCase))
                report.TechnicianTotal += dto.TotalTasks;
            else if (string.Equals(primaryRole, "Student", StringComparison.OrdinalIgnoreCase))
                report.StudentTotal += dto.TotalTasks;
        }

        report.TotalUsers = report.Users.Count;
        report.TotalTasks = report.Users.Sum(u => u.TotalTasks);
        report.Users = report.Users
            .OrderBy(u => u.RoleName).ThenBy(u => u.FullName)
            .ToList();

        return report;
    }

    private static (DateTime startUtc, DateTime endUtc) GetIctDayWindowUtc(DateOnly date)
    {
        // ICT = UTC+7, không DST. Cửa sổ [00:00 ICT, 00:00 ICT ngày mai).
        var startIct = date.ToDateTime(TimeOnly.MinValue);
        var endIct = startIct.AddDays(1);
        return (
            DateTime.SpecifyKind(startIct.AddHours(-VietnamTime.VietnamUtcOffsetHours), DateTimeKind.Utc),
            DateTime.SpecifyKind(endIct.AddHours(-VietnamTime.VietnamUtcOffsetHours), DateTimeKind.Utc)
        );
    }
}