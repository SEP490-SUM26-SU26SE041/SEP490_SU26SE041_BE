using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartFarmSEP490.API.Helpers;
using SmartFarmSEP490.Model.DTOs;
using SmartFarmSEP490.Service.Interfaces.Dashboard;

namespace SmartFarmSEP490.API.Controllers;

[Route("api/dashboard")]
[ApiController]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;
    private readonly IComparisonService _comparisonService;
    private readonly IReportExportService _reportExportService;

    public DashboardController(
        IDashboardService dashboardService,
        IComparisonService comparisonService,
        IReportExportService reportExportService)
    {
        _dashboardService = dashboardService;
        _comparisonService = comparisonService;
        _reportExportService = reportExportService;
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)
        ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new UnauthorizedAccessException("User identifier claim not found."));

    private bool IsResearcher() => User.FindFirstValue(ClaimTypes.Role) == "Researcher";
    private bool IsManager() => User.FindFirstValue(ClaimTypes.Role) == "Manager";

    // ========== T24: Real-time Monitoring Dashboard ==========

    /// <summary>
    /// Get overall dashboard overview - Available for Researcher and Manager
    /// </summary>
    [HttpGet("overview")]
    [Authorize(Roles = "Researcher,Manager")]
    public async Task<IActionResult> GetDashboardOverview([FromQuery] Guid? farmId = null)
    {
        var overview = await _dashboardService.GetDashboardOverviewAsync(farmId);
        return Ok(overview);
    }

    /// <summary>
    /// Get health status of all farms - Available for Researcher and Manager
    /// </summary>
    [HttpGet("farms/health")]
    [Authorize(Roles = "Researcher,Manager")]
    public async Task<IActionResult> GetFarmsHealth()
    {
        var farms = await _dashboardService.GetFarmHealthListAsync();
        return Ok(farms);
    }

    /// <summary>
    /// Get health status of a specific farm - Available for Researcher and Manager
    /// </summary>
    [HttpGet("farms/{farmId:guid}/health")]
    [Authorize(Roles = "Researcher,Manager")]
    public async Task<IActionResult> GetFarmHealth(Guid farmId)
    {
        var health = await _dashboardService.GetFarmHealthAsync(farmId);
        return health == null ? NotFound("Farm not found") : Ok(health);
    }

    /// <summary>
    /// Get latest sensor readings - Available for Researcher and Manager
    /// </summary>
    [HttpGet("sensors/latest")]
    [Authorize(Roles = "Researcher,Manager")]
    public async Task<IActionResult> GetLatestSensorReadings(
        [FromQuery] Guid? farmId = null,
        [FromQuery] Guid? experimentId = null)
    {
        var readings = await _dashboardService.GetLatestSensorReadingsAsync(farmId, experimentId);
        return Ok(readings);
    }

    /// <summary>
    /// Get sensor reading history - Available for Researcher and Manager
    /// </summary>
    [HttpGet("sensors/{sensorId:guid}/history")]
    [Authorize(Roles = "Researcher,Manager")]
    public async Task<IActionResult> GetSensorHistory(
        Guid sensorId,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] int limit = 100)
    {
        var history = await _dashboardService.GetSensorHistoryAsync(sensorId, fromDate, toDate, limit);
        return Ok(history);
    }

    /// <summary>
    /// Get active alerts - Available for Researcher and Manager
    /// </summary>
    [HttpGet("alerts")]
    [Authorize(Roles = "Researcher,Manager")]
    public async Task<IActionResult> GetActiveAlerts([FromQuery] Guid? experimentId = null)
    {
        var alerts = await _dashboardService.GetActiveAlertsAsync(experimentId);
        return Ok(alerts);
    }

    /// <summary>
    /// Get experiment statuses - Available for Researcher and Manager
    /// </summary>
    [HttpGet("experiments/status")]
    [Authorize(Roles = "Researcher,Manager")]
    public async Task<IActionResult> GetExperimentStatuses([FromQuery] Guid? farmId = null)
    {
        var statuses = await _dashboardService.GetExperimentStatusesAsync(farmId);
        return Ok(statuses);
    }

    // ========== T25: KPIs and Personnel Performance ==========

    /// <summary>
    /// Get dashboard KPIs - Available for Researcher and Manager
    /// </summary>
    [HttpGet("kpis")]
    [Authorize(Roles = "Researcher,Manager")]
    public async Task<IActionResult> GetKpis(
        [FromQuery] Guid? farmId = null,
        [FromQuery] Guid? experimentId = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        var kpis = await _dashboardService.GetKpisAsync(farmId, experimentId, fromDate, toDate);
        return Ok(kpis);
    }

    /// <summary>
    /// Get personnel performance metrics - Available for Researcher and Manager
    /// </summary>
    [HttpGet("personnel/performance")]
    [Authorize(Roles = "Researcher,Manager")]
    public async Task<IActionResult> GetPersonnelPerformance(
        [FromQuery] Guid? farmId = null,
        [FromQuery] Guid? experimentId = null)
    {
        var performance = await _dashboardService.GetPersonnelPerformanceAsync(farmId, experimentId);
        return Ok(performance);
    }

    /// <summary>
    /// Get specific personnel performance - Available for Researcher and Manager
    /// </summary>
    [HttpGet("personnel/{userId:guid}/performance")]
    [Authorize(Roles = "Researcher,Manager")]
    public async Task<IActionResult> GetPersonnelPerformanceById(Guid userId)
    {
        var performance = await _dashboardService.GetPersonnelPerformanceByIdAsync(userId);
        return performance == null ? NotFound("User not found") : Ok(performance);
    }

    /// <summary>
    /// Get experiment progress - Available for Researcher and Manager
    /// </summary>
    [HttpGet("experiments/progress")]
    [Authorize(Roles = "Researcher,Manager")]
    public async Task<IActionResult> GetExperimentProgress([FromQuery] Guid? farmId = null)
    {
        var progress = await _dashboardService.GetExperimentProgressAsync(farmId);
        return Ok(progress);
    }

    /// <summary>
    /// Get specific experiment progress - Available for Researcher and Manager
    /// </summary>
    [HttpGet("experiments/{experimentId:guid}/progress")]
    [Authorize(Roles = "Researcher,Manager")]
    public async Task<IActionResult> GetExperimentProgressById(Guid experimentId)
    {
        var progress = await _dashboardService.GetExperimentProgressByIdAsync(experimentId);
        return progress == null ? NotFound("Experiment not found") : Ok(progress);
    }

    // ========== T26: Cultivation Method Comparison ==========

    /// <summary>
    /// Get cultivation comparison for an experiment - Available for Researcher
    /// </summary>
    [HttpGet("experiments/{experimentId:guid}/comparison")]
    [Authorize(Roles = "Researcher")]
    public async Task<IActionResult> GetCultivationComparison(Guid experimentId)
    {
        var comparison = await _comparisonService.GetComparisonAsync(experimentId);
        return comparison == null ? NotFound("Experiment not found") : Ok(comparison);
    }

    /// <summary>
    /// Get all completed experiment comparisons for a farm - Available for Researcher
    /// </summary>
    [HttpGet("comparisons")]
    [Authorize(Roles = "Researcher")]
    public async Task<IActionResult> GetAllComparisons([FromQuery] Guid? farmId = null)
    {
        var comparisons = await _comparisonService.GetAllComparisonsAsync(farmId);
        return Ok(comparisons);
    }

    /// <summary>
    /// So sánh chỉ số tăng trưởng trung bình giữa 2 nhóm theo từng giai đoạn
    /// (có thể là theo chiều cao, số lá, hoặc tất cả metric).
    /// Input: experimentId (route), groupAId & groupBId (query), metricName (query, optional).
    /// </summary>
    [HttpGet("experiments/{experimentId:guid}/group-comparison")]
    // [Authorize(Roles = "Researcher,Manager")]
    public async Task<IActionResult> GetGroupGrowthComparison(
        Guid experimentId,
        [FromQuery] Guid groupAId,
        [FromQuery] Guid groupBId,
        [FromQuery] string? metricName = null)
    {
        if (groupAId == Guid.Empty || groupBId == Guid.Empty)
            return BadRequest(ApiResponse.Error("groupAId và groupBId là bắt buộc."));
        if (groupAId == groupBId)
            return BadRequest(ApiResponse.Error("groupAId và groupBId phải khác nhau."));

        try
        {
            var result = await _comparisonService.GetGroupGrowthComparisonAsync(
                experimentId, groupAId, groupBId, metricName);
            return result == null
                ? NotFound(ApiResponse.Error("Không tìm thấy thực nghiệm."))
                : Ok(ApiResponse<GroupGrowthComparisonDto>.Ok(result, "So sánh chỉ số tăng trưởng giữa 2 nhóm theo giai đoạn."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.Error(ex.Message));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse.Error(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse.Error(ex.Message));
        }
    }

    // ========== T27: Report Export ==========

    /// <summary>
    /// Generate experiment report - Available for Researcher
    /// </summary>
    [HttpPost("experiments/{experimentId:guid}/reports")]
    [Authorize(Roles = "Researcher")]
    public async Task<IActionResult> GenerateReport(Guid experimentId, [FromBody] ExportReportRequestDto request)
    {
        if (request.ExperimentId != experimentId)
        {
            return BadRequest("Experiment ID mismatch");
        }

        var result = await _reportExportService.GenerateReportAsync(request, GetUserId());
        return result.Status == "Success"
            ? Ok(result)
            : BadRequest(result);
    }

    /// <summary>
    /// Get all reports for an experiment - Available for Researcher
    /// </summary>
    [HttpGet("experiments/{experimentId:guid}/reports")]
    [Authorize(Roles = "Researcher")]
    public async Task<IActionResult> GetExperimentReports(Guid experimentId)
    {
        var reports = await _reportExportService.GetExperimentReportsAsync(experimentId);
        return Ok(reports);
    }

    /// <summary>
    /// Get specific report - Available for Researcher
    /// </summary>
    [HttpGet("reports/{reportId:guid}")]
    [Authorize(Roles = "Researcher")]
    public async Task<IActionResult> GetReport(Guid reportId)
    {
        var report = await _reportExportService.GetReportByIdAsync(reportId);
        return report == null ? NotFound("Report not found") : Ok(report);
    }

    /// <summary>
    /// Delete a report - Available for Researcher
    /// </summary>
    [HttpDelete("reports/{reportId:guid}")]
    [Authorize(Roles = "Researcher")]
    public async Task<IActionResult> DeleteReport(Guid reportId)
    {
        var deleted = await _reportExportService.DeleteReportAsync(reportId);
        return deleted ? NoContent() : NotFound("Report not found");
    }
}
