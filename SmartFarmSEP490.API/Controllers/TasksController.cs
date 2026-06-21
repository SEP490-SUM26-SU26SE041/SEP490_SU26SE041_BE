using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartFarmSEP490.Model.DTOs;
using SmartFarmSEP490.Repository.Interfaces.Experiments;
using SmartFarmSEP490.Service.Interfaces.Tasks;

namespace SmartFarmSEP490.API.Controllers;

[Route("api/tasks")]
[ApiController]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;
    private readonly IExperimentRepository _experimentRepository;

    public TasksController(
        ITaskService taskService,
        IExperimentRepository experimentRepository)
    {
        _taskService = taskService;
        _experimentRepository = experimentRepository;
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)
        ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new UnauthorizedAccessException("User identifier claim not found."));
    private bool IsResearcher() => User.FindFirstValue(ClaimTypes.Role) == "Researcher";

    private async Task<bool> IsExperimentOwnerAsync(Guid experimentId)
    {
        var exp = await _experimentRepository.GetByIdAsync(experimentId);
        return exp != null && exp.ResearcherId == GetUserId();
    }

    private async Task<bool> CanManageTaskAsync(Guid taskId)
    {
        if (IsResearcher()) return true;
        var task = await _taskService.GetByIdAsync(taskId);
        if (task == null) return false;
        return await IsExperimentOwnerAsync(task.ExperimentId);
    }

    // ========== Tasks CRUD ==========

    [HttpPost]
    public async Task<IActionResult> CreateTask([FromBody] CreateTaskDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        if (!IsResearcher()) return Forbid();
        if (!await IsExperimentOwnerAsync(dto.ExperimentId)) return Forbid();

        var result = await _taskService.CreateAsync(dto, GetUserId());
        return result == null ? BadRequest("Experiment not found or invalid data.") : CreatedAtAction(nameof(GetTaskById), new { id = result.Id }, result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetTaskById(Guid id)
    {
        var result = await _taskService.GetByIdAsync(id);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllTasks(
        [FromQuery] Guid? experimentId,
        [FromQuery] Guid? assigneeId)
    {
        if (experimentId.HasValue)
        {
            if (!await IsExperimentOwnerAsync(experimentId.Value)) return Forbid();
            return Ok(await _taskService.GetByExperimentAsync(experimentId.Value));
        }

        if (assigneeId.HasValue)
        {
            return Ok(await _taskService.GetByAssigneeAsync(assigneeId.Value));
        }

        if (!IsResearcher()) return Forbid();
        return Ok(await _taskService.GetAllAsync());
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateTask(Guid id, [FromBody] UpdateTaskDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var task = await _taskService.GetByIdAsync(id);
        if (task == null) return NotFound();
        if (!await IsExperimentOwnerAsync(task.ExperimentId)) return Forbid();

        var result = await _taskService.UpdateAsync(id, dto, GetUserId());
        return result == null ? NotFound() : Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteTask(Guid id)
    {
        var task = await _taskService.GetByIdAsync(id);
        if (task == null) return NotFound();
        if (!await IsExperimentOwnerAsync(task.ExperimentId)) return Forbid();

        await _taskService.DeleteAsync(id);
        return NoContent();
    }

    // ========== Task Status ==========

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateTaskStatus(Guid id, [FromBody] UpdateExperimentStatusDto dto)
    {
        var task = await _taskService.GetByIdAsync(id);
        if (task == null) return NotFound();

        var hasAccess = IsResearcher() || (task.AssignedTo.HasValue && task.AssignedTo.Value == GetUserId());
        if (!hasAccess) return Forbid();

        var result = await _taskService.UpdateTaskStatusAsync(id, dto.Status, GetUserId());
        return result == null ? NotFound() : Ok(result);
    }

    // ========== Task Assignment ==========

    /// <summary>
    /// Gan mot task cho mot nguoi dung (Technician/Student) co kiem tra skill.
    /// </summary>
    [HttpPost("assign")]
    public async Task<IActionResult> AssignTask([FromBody] AssignTaskDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        if (!IsResearcher()) return Forbid();

        var task = await _taskService.GetByIdAsync(dto.TaskId);
        if (task == null) return NotFound("Task not found.");

        if (!await IsExperimentOwnerAsync(task.ExperimentId)) return Forbid();

        var result = await _taskService.AssignTaskAsync(dto, GetUserId());
        return result == null
            ? BadRequest("Assignment failed: assignee not found, assignee has wrong role, or already assigned to this task.")
            : Ok(result);
    }

    /// <summary>
    /// Chuyen giao task tu nguoi dung hien tai sang nguoi dung khac.
    /// </summary>
    [HttpPost("reassign")]
    public async Task<IActionResult> ReassignTask([FromBody] ReassignTaskDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        if (!IsResearcher()) return Forbid();

        var task = await _taskService.GetByIdAsync(dto.TaskId);
        if (task == null) return NotFound("Task not found.");

        if (!await IsExperimentOwnerAsync(task.ExperimentId)) return Forbid();

        var result = await _taskService.ReassignTaskAsync(dto, GetUserId());
        return result == null
            ? BadRequest("Reassignment failed: assignee not found or has wrong role.")
            : Ok(result);
    }

    /// <summary>
    /// Cap nhat trang thai assignment (Assigned -> Completed/Cancelled/Resigned).
    /// </summary>
    [HttpPatch("assignments/status")]
    public async Task<IActionResult> UpdateAssignmentStatus([FromBody] UpdateTaskAssignmentStatusDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var assignment = await _taskService.GetTaskAssignmentsAsync(dto.AssignmentId == Guid.Empty ? Guid.Empty : dto.AssignmentId);
        var result = await _taskService.UpdateAssignmentStatusAsync(dto);
        return result == null ? NotFound("Assignment not found.") : Ok(result);
    }

    /// <summary>
    /// Lay danh sach assignment cua mot task.
    /// </summary>
    [HttpGet("{taskId:guid}/assignments")]
    public async Task<IActionResult> GetTaskAssignments(Guid taskId)
    {
        var task = await _taskService.GetByIdAsync(taskId);
        if (task == null) return NotFound();

        if (!await IsExperimentOwnerAsync(task.ExperimentId)) return Forbid();

        return Ok(await _taskService.GetTaskAssignmentsAsync(taskId));
    }

    /// <summary>
    /// Lay tat ca assignment cua mot nguoi dung.
    /// </summary>
    [HttpGet("assignments/my")]
    public async Task<IActionResult> GetMyAssignments()
    {
        var userId = GetUserId();
        return Ok(await _taskService.GetAssignmentsByAssigneeAsync(userId));
    }

    // ========== Skill Matching ==========

    /// <summary>
    /// Tim danh sach nguoi dung phu hop voi task dua tren skill requirements.
    /// Chi tra ve Technician va Student co skill phu hop.
    /// </summary>
    [HttpGet("{taskId:guid}/skill-matches")]
    public async Task<IActionResult> FindSkillMatches(Guid taskId)
    {
        var task = await _taskService.GetByIdAsync(taskId);
        if (task == null) return NotFound();

        if (!await IsExperimentOwnerAsync(task.ExperimentId)) return Forbid();

        return Ok(await _taskService.FindMatchingUsersAsync(taskId));
    }
}
