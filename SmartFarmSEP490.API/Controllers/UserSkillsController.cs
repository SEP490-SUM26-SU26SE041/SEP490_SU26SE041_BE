using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartFarmSEP490.Model.DTOs;
using SmartFarmSEP490.Service.Interfaces.Skills;

namespace SmartFarmSEP490.API.Controllers;

/// <summary>
/// Quản lý quan hệ User ↔ Skill (UserSkill).
/// - GET (ai đăng nhập cũng xem được — FE filter, hiển thị badge, ...).
/// - POST/PUT/DELETE — Admin only.
/// Routes:
///   GET    /api/user-skills
///   GET    /api/user-skills/users/{userId}
///   GET    /api/user-skills/skills/{skillId}/users
///   GET    /api/user-skills/{userId}/{skillId}
///   POST   /api/user-skills
///   PUT    /api/user-skills/{userId}/{skillId}
///   DELETE /api/user-skills/{userId}/{skillId}
/// </summary>
[Route("api/user-skills")]
[ApiController]
[Authorize]
public class UserSkillsController : ControllerBase
{
    private readonly IUserSkillService _userSkillService;

    public UserSkillsController(IUserSkillService userSkillService)
    {
        _userSkillService = userSkillService;
    }

    /// <summary>GET /api/user-skills — liệt kê toàn bộ quan hệ user-skill.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(await _userSkillService.GetAllAsync());

    /// <summary>GET /api/user-skills/users/{userId} — lấy skill của 1 user.</summary>
    [HttpGet("users/{userId:guid}")]
    public async Task<IActionResult> GetByUser(Guid userId)
        => Ok(await _userSkillService.GetByUserAsync(userId));

    /// <summary>GET /api/user-skills/skills/{skillId}/users — lấy user có 1 skill.</summary>
    [HttpGet("skills/{skillId:guid}/users")]
    public async Task<IActionResult> GetBySkill(Guid skillId)
        => Ok(await _userSkillService.GetBySkillAsync(skillId));

    /// <summary>GET /api/user-skills/{userId}/{skillId} — lấy 1 quan hệ user-skill cụ thể.</summary>
    [HttpGet("{userId:guid}/{skillId:guid}")]
    public async Task<IActionResult> GetByKey(Guid userId, Guid skillId)
    {
        var result = await _userSkillService.GetByKeyAsync(userId, skillId);
        return result == null ? NotFound() : Ok(result);
    }

    /// <summary>POST /api/user-skills — Admin only.</summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateUserSkillDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        try
        {
            var result = await _userSkillService.CreateAsync(dto);
            return result == null
                ? BadRequest()
                : CreatedAtAction(nameof(GetByKey),
                    new { userId = result.UserId, skillId = result.SkillId }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>PUT /api/user-skills/{userId}/{skillId} — Admin only.</summary>
    [HttpPut("{userId:guid}/{skillId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid userId, Guid skillId, [FromBody] UpdateUserSkillDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _userSkillService.UpdateAsync(userId, skillId, dto);
        return result == null ? NotFound() : Ok(result);
    }

    /// <summary>DELETE /api/user-skills/{userId}/{skillId} — Admin only.</summary>
    [HttpDelete("{userId:guid}/{skillId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid userId, Guid skillId)
    {
        var ok = await _userSkillService.DeleteAsync(userId, skillId);
        return ok ? NoContent() : NotFound();
    }
}
