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

    // ========== Task Generation ==========

    /// <summary>
    /// Sinh task tu CareSchedule cua 1 Stage.
    /// </summary>
    [HttpPost("generate-by-stage/{stageId:guid}")]
    public async Task<IActionResult> GenerateByStage(Guid stageId)
    {
        if (!IsResearcher()) return Forbid();
        var result = await _taskService.GenerateByStageAsync(stageId, GetUserId());

        if (result.HasError)
        {
            return BadRequest(new
            {
                stageId = result.StageId,
                stageName = result.StageName,
                totalSchedules = result.TotalSchedules,
                existingTasksCount = result.ExistingTasksCount,
                message = result.Message
            });
        }

        return Ok(result);
    }

    /// <summary>
    /// Sinh toan bo task cua Experiment (tat ca Stage -> CareSchedule).
    /// </summary>
    [HttpPost("generate-by-experiment/{experimentId:guid}")]
    public async Task<IActionResult> GenerateByExperiment(Guid experimentId)
    {
        if (!IsResearcher()) return Forbid();
        if (!await IsExperimentOwnerAsync(experimentId)) return Forbid();
        var result = await _taskService.GenerateByExperimentAsync(experimentId, GetUserId());

        if (result.HasError)
        {
            return BadRequest(new
            {
                experimentId = result.ExperimentId,
                totalStages = result.TotalStages,
                totalSchedules = result.TotalSchedules,
                stagesSkipped = result.StagesSkipped,
                message = result.Message,
                stageResults = result.StageResults
            });
        }

        return Ok(result);
    }

    // ========== Task CRUD ==========

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

    // ========== Task Filter ==========

    /// <summary>
    /// Get Tasks By Experiment
    /// </summary>
    [HttpGet("experiment/{experimentId:guid}")]
    public async Task<IActionResult> GetByExperiment(Guid experimentId)
    {
        if (!await IsExperimentOwnerAsync(experimentId)) return Forbid();
        return Ok(await _taskService.GetByExperimentAsync(experimentId));
    }

    /// <summary>
    /// Get Tasks By Stage
    /// </summary>
    [HttpGet("stage/{stageId:guid}")]
    public async Task<IActionResult> GetByStage(Guid stageId)
    {
        return Ok(await _taskService.GetByStageAsync(stageId));
    }

    /// <summary>
    /// Get Tasks By Batch
    /// </summary>
    [HttpGet("batch/{batchId:guid}")]
    public async Task<IActionResult> GetByBatch(Guid batchId)
    {
        return Ok(await _taskService.GetByBatchAsync(batchId));
    }

    /// <summary>
    /// Get Tasks By User
    /// </summary>
    [HttpGet("user/{userId:guid}")]
    public async Task<IActionResult> GetByUser(Guid userId)
    {
        if (!IsResearcher()) return Forbid();
        return Ok(await _taskService.GetByAssigneeAsync(userId));
    }

    /// <summary>
    /// Get My Tasks - JWT User
    /// </summary>
    [HttpGet("my")]
    public async Task<IActionResult> GetMyTasks()
    {
        return Ok(await _taskService.GetByAssigneeAsync(GetUserId()));
    }

    /// <summary>
    /// Get Today Tasks - Mobile dùng nhiều nhất
    /// </summary>
    [HttpGet("today")]
    public async Task<IActionResult> GetTodayTasks()
    {
        return Ok(await _taskService.GetTodayTasksAsync(GetUserId()));
    }

    /// <summary>
    /// Get Upcoming Tasks
    /// </summary>
    [HttpGet("upcoming")]
    public async Task<IActionResult> GetUpcomingTasks([FromQuery] int days = 7)
    {
        return Ok(await _taskService.GetUpcomingTasksAsync(GetUserId(), days));
    }

    /// <summary>
    /// Get Overdue Tasks
    /// </summary>
    [HttpGet("overdue")]
    public async Task<IActionResult> GetOverdueTasks()
    {
        return Ok(await _taskService.GetOverdueTasksAsync(GetUserId()));
    }

    // ========== Task Status ==========

    /// <summary>
    /// Start Task: Pending -> InProgress
    /// </summary>
    [HttpPatch("{id:guid}/start")]
    public async Task<IActionResult> StartTask(Guid id)
    {
        var task = await _taskService.GetByIdAsync(id);
        if (task == null) return NotFound();

        var hasAccess = task.AssignedTo.HasValue && task.AssignedTo.Value == GetUserId();
        if (!hasAccess) return Forbid();

        var result = await _taskService.UpdateTaskStatusAsync(id, "InProgress", GetUserId());
        return result == null ? NotFound() : Ok(result);
    }

    /// <summary>
    /// Complete Task: InProgress -> Completed
    /// </summary>
    [HttpPatch("{id:guid}/complete")]
    public async Task<IActionResult> CompleteTask(Guid id)
    {
        var task = await _taskService.GetByIdAsync(id);
        if (task == null) return NotFound();

        var hasAccess = task.AssignedTo.HasValue && task.AssignedTo.Value == GetUserId();
        if (!hasAccess) return Forbid();

        var result = await _taskService.UpdateTaskStatusAsync(id, "Completed", GetUserId());
        return result == null ? NotFound() : Ok(result);
    }

    /// <summary>
    /// Cancel Task
    /// </summary>
    [HttpPatch("{id:guid}/cancel")]
    public async Task<IActionResult> CancelTask(Guid id)
    {
        var task = await _taskService.GetByIdAsync(id);
        if (task == null) return NotFound();

        if (!await IsExperimentOwnerAsync(task.ExperimentId)) return Forbid();

        var result = await _taskService.UpdateTaskStatusAsync(id, "Cancelled", GetUserId());
        return result == null ? NotFound() : Ok(result);
    }

    /// <summary>
    /// Update Status (generic)
    /// </summary>
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

    [HttpPatch("assignments/status")]
    public async Task<IActionResult> UpdateAssignmentStatus([FromBody] UpdateTaskAssignmentStatusDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var result = await _taskService.UpdateAssignmentStatusAsync(dto);
        return result == null ? NotFound("Assignment not found.") : Ok(result);
    }

    [HttpGet("{taskId:guid}/assignments")]
    public async Task<IActionResult> GetTaskAssignments(Guid taskId)
    {
        var task = await _taskService.GetByIdAsync(taskId);
        if (task == null) return NotFound();

        if (!await IsExperimentOwnerAsync(task.ExperimentId)) return Forbid();

        return Ok(await _taskService.GetTaskAssignmentsAsync(taskId));
    }

    [HttpGet("assignments/my")]
    public async Task<IActionResult> GetMyAssignments()
    {
        return Ok(await _taskService.GetAssignmentsByAssigneeAsync(GetUserId()));
    }

    // ========== Skill Matching ==========

    [HttpGet("{taskId:guid}/skill-matches")]
    public async Task<IActionResult> FindSkillMatches(Guid taskId)
    {
        var task = await _taskService.GetByIdAsync(taskId);
        if (task == null) return NotFound();

        if (!await IsExperimentOwnerAsync(task.ExperimentId)) return Forbid();

        return Ok(await _taskService.FindMatchingUsersAsync(taskId));
    }
}
