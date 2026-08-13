using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartFarmSEP490.Model.DTOs;
using SmartFarmSEP490.Service.Interfaces.Tasks;

namespace SmartFarmSEP490.API.Controllers;

[Route("api")]
[ApiController]
[Authorize]
public class StageStatisticsController : ControllerBase
{
    private readonly IMeasurementStatisticsService _statsService;

    public StageStatisticsController(IMeasurementStatisticsService statsService)
    {
        _statsService = statsService;
    }

    /// <summary>
    /// Aggregate measurement statistics by group for a stage.
    /// Returns avg/min/max/stddev/median/q1/q3 per (group, metric),
    /// plus cross-group comparison and growth over time.
    /// </summary>
    [HttpGet("experiments/stages/{stageId:guid}/statistics")]
    public async Task<IActionResult> GetStageStatistics(
        Guid stageId,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] Guid? groupId = null)
    {
        try
        {
            var stats = await _statsService.GetStageStatisticsAsync(stageId, fromDate, toDate, groupId);
            return Ok(new { success = true, message = "Thống kê giai đoạn.", data = stats });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Aggregate measurement statistics across the entire experiment
    /// (used by Evaluation stage to summarize all metrics).
    /// </summary>
    [HttpGet("experiments/{experimentId:guid}/statistics")]
    public async Task<IActionResult> GetExperimentStatistics(
        Guid experimentId,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        try
        {
            var stats = await _statsService.GetExperimentOverallStatisticsAsync(experimentId, fromDate, toDate);
            return Ok(new { success = true, message = "Thống kê tổng hợp thực nghiệm.", data = stats });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Export stage statistics as CSV or XLSX (XML Spreadsheet 2003).
    /// Researcher downloads the file directly.
    /// </summary>
    [HttpPost("experiments/stages/{stageId:guid}/statistics/export")]
    public async Task<IActionResult> ExportStageStatistics(
        Guid stageId,
        [FromBody] StageStatisticsExportRequestDto? request)
    {
        request ??= new StageStatisticsExportRequestDto();
        request.StageId = stageId;

        try
        {
            var bytes = await _statsService.ExportStageStatisticsAsync(request);
            var format = (request.Format ?? "csv").Trim().ToLowerInvariant();
            var contentType = format == "xlsx"
                ? "application/vnd.ms-excel"
                : "text/csv";
            var fileName = $"stage-statistics-{stageId}-{DateTime.UtcNow:yyyyMMddHHmmss}.{format}";

            return File(bytes, contentType, fileName);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Validate a value against the metric's unit + target rules.
    /// Returns a list of validation errors (empty list = valid).
    /// </summary>
    [HttpGet("measurement-definitions/{definitionId:guid}/validate")]
    public async Task<IActionResult> ValidateValue(Guid definitionId, [FromQuery] decimal value)
    {
        var errors = await _statsService.ValidateMeasurementValueAsync(definitionId, value);
        return Ok(new { success = errors.Count == 0, errors });
    }
}
