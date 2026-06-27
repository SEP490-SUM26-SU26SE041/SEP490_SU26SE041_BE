using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartFarmSEP490.Model.DTOs;
using SmartFarmSEP490.Service.Interfaces.Tasks;

namespace SmartFarmSEP490.API.Controllers;

[Route("api/task-reports")]
[ApiController]
[Authorize]
public class TaskReportsController : ControllerBase
{
    private readonly ITaskReportService _reportService;
    private readonly ITaskService _taskService;

    public TaskReportsController(ITaskReportService reportService, ITaskService taskService)
    {
        _reportService = reportService;
        _taskService = taskService;
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)
        ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new UnauthorizedAccessException("User identifier claim not found."));

    /// <summary>
    /// Create Task Report - dung cho Observation, Watering, Fertilizing, Inspection
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateReport([FromBody] CreateTaskReportDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var task = await _taskService.GetByIdAsync(dto.TaskId);
        if (task == null) return NotFound("Task not found.");

        var hasAccess = task.AssignedTo.HasValue && task.AssignedTo.Value == GetUserId();
        if (!hasAccess) return Forbid();

        var result = await _reportService.CreateAsync(dto, GetUserId());
        return result == null ? BadRequest("Failed to create report.") : CreatedAtAction(nameof(GetByTask), new { taskId = dto.TaskId }, result);
    }

    /// <summary>
    /// Update Task Report
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateReport(Guid id, [FromBody] UpdateTaskReportDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var existing = await _reportService.GetByIdAsync(id);
        if (existing == null) return NotFound();

        if (existing.ReporterId != GetUserId()) return Forbid();

        var result = await _reportService.UpdateAsync(id, dto);
        return result == null ? NotFound() : Ok(result);
    }

    /// <summary>
    /// Get Report By Task
    /// </summary>
    [HttpGet("task/{taskId:guid}")]
    public async Task<IActionResult> GetByTask(Guid taskId)
    {
        return Ok(await _reportService.GetByTaskIdAsync(taskId));
    }

    /// <summary>
    /// Get Reports By Batch
    /// </summary>
    [HttpGet("batch/{batchId:guid}")]
    public async Task<IActionResult> GetByBatch(Guid batchId)
    {
        return Ok(await _reportService.GetByBatchIdAsync(batchId));
    }

    /// <summary>
    /// Get Report By Id
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _reportService.GetByIdAsync(id);
        return result == null ? NotFound() : Ok(result);
    }
}
