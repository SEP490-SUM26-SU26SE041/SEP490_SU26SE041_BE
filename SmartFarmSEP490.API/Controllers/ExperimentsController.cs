using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartFarmSEP490.Model.DTOs;
using SmartFarmSEP490.Service.Interfaces.Experiments;

namespace SmartFarmSEP490.API.Controllers;

[Route("api/experiments")]
[ApiController]
[Authorize]
public class ExperimentsController : ControllerBase
{
    private readonly IExperimentService _experimentService;
    private readonly IExperimentStageService _stageService;
    private readonly IExperimentGroupService _groupService;
    private readonly IExperimentDesignService _designService;
    private readonly IMeasurementDefinitionService _measurementService;
    private readonly IProcedureTemplateService _templateService;
    private readonly ICareScheduleService _scheduleService;

    public ExperimentsController(
        IExperimentService experimentService,
        IExperimentStageService stageService,
        IExperimentGroupService groupService,
        IExperimentDesignService designService,
        IMeasurementDefinitionService measurementService,
        IProcedureTemplateService templateService,
        ICareScheduleService scheduleService)
    {
        _experimentService = experimentService;
        _stageService = stageService;
        _groupService = groupService;
        _designService = designService;
        _measurementService = measurementService;
        _templateService = templateService;
        _scheduleService = scheduleService;
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // ========== Experiments ==========

    [HttpPost]
    public async Task<IActionResult> CreateExperiment([FromBody] CreateExperimentDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var role = User.FindFirstValue(ClaimTypes.Role);
        if (role != "Researcher" && role != "Admin")
            return Forbid();

        var result = await _experimentService.CreateAsync(dto, GetUserId());
        return CreatedAtAction(nameof(GetExperimentById), new { id = result.Id }, result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetExperimentById(Guid id)
    {
        var result = await _experimentService.GetByIdAsync(id);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllExperiments(
        [FromQuery] Guid? researcherId,
        [FromQuery] Guid? farmId)
    {
        if (researcherId.HasValue)
            return Ok(await _experimentService.GetByResearcherAsync(researcherId.Value));
        if (farmId.HasValue)
            return Ok(await _experimentService.GetByFarmAsync(farmId.Value));
        return Ok(await _experimentService.GetAllAsync());
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateExperiment(Guid id, [FromBody] UpdateExperimentDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        try
        {
            var result = await _experimentService.UpdateAsync(id, dto, GetUserId());
            if (result == null) return NotFound();
            return Ok(result);
        }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateStatusDto dto)
    {
        var role = User.FindFirstValue(ClaimTypes.Role);
        if (role != "Researcher" && role != "Manager" && role != "Admin")
            return Forbid();

        var result = await _experimentService.UpdateStatusAsync(id, dto.Status, GetUserId());
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteExperiment(Guid id)
    {
        await _experimentService.DeleteAsync(id);
        return NoContent();
    }

    // ========== Experiment Stages ==========

    [HttpPost("{experimentId:guid}/stages")]
    public async Task<IActionResult> CreateStage(Guid experimentId, [FromBody] CreateExperimentStageDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _stageService.CreateAsync(experimentId, dto);
        return CreatedAtAction(nameof(GetStageById), new { id = result.Id }, result);
    }

    [HttpGet("{experimentId:guid}/stages")]
    public async Task<IActionResult> GetStagesByExperiment(Guid experimentId)
        => Ok(await _stageService.GetByExperimentAsync(experimentId));

    [HttpPut("stages/{id:guid}")]
    public async Task<IActionResult> UpdateStage(Guid id, [FromBody] UpdateExperimentStageDto dto)
    {
        var result = await _stageService.UpdateAsync(id, dto);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpDelete("stages/{id:guid}")]
    public async Task<IActionResult> DeleteStage(Guid id)
    {
        await _stageService.DeleteAsync(id);
        return NoContent();
    }

    // ========== Experiment Groups ==========

    [HttpPost("{experimentId:guid}/groups")]
    public async Task<IActionResult> CreateGroup(Guid experimentId, [FromBody] CreateExperimentGroupDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _groupService.CreateAsync(experimentId, dto);
        return CreatedAtAction(nameof(GetGroupById), new { id = result.Id }, result);
    }

    [HttpGet("{experimentId:guid}/groups")]
    public async Task<IActionResult> GetGroupsByExperiment(Guid experimentId)
        => Ok(await _groupService.GetByExperimentAsync(experimentId));

    [HttpPut("groups/{id:guid}")]
    public async Task<IActionResult> UpdateGroup(Guid id, [FromBody] UpdateExperimentGroupDto dto)
    {
        var result = await _groupService.UpdateAsync(id, dto);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpDelete("groups/{id:guid}")]
    public async Task<IActionResult> DeleteGroup(Guid id)
    {
        await _groupService.DeleteAsync(id);
        return NoContent();
    }

    // ========== Experiment Design ==========

    [HttpPost("{experimentId:guid}/design")]
    public async Task<IActionResult> CreateDesign(Guid experimentId, [FromBody] CreateExperimentDesignDto dto)
    {
        var result = await _designService.CreateAsync(experimentId, dto);
        return Ok(result);
    }

    [HttpGet("{experimentId:guid}/design")]
    public async Task<IActionResult> GetDesignByExperiment(Guid experimentId)
    {
        var result = await _designService.GetByExperimentAsync(experimentId);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPut("{experimentId:guid}/design")]
    public async Task<IActionResult> UpdateDesign(Guid experimentId, [FromBody] UpdateExperimentDesignDto dto)
    {
        var result = await _designService.UpdateAsync(experimentId, dto);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpDelete("{experimentId:guid}/design")]
    public async Task<IActionResult> DeleteDesign(Guid experimentId)
    {
        await _designService.DeleteAsync(experimentId);
        return NoContent();
    }

    // ========== Measurement Definitions ==========

    [HttpPost("{experimentId:guid}/measurements")]
    public async Task<IActionResult> CreateMeasurement(Guid experimentId, [FromBody] CreateMeasurementDefinitionDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _measurementService.CreateAsync(experimentId, dto);
        return CreatedAtAction(nameof(GetMeasurementById), new { id = result.Id }, result);
    }

    [HttpGet("{experimentId:guid}/measurements")]
    public async Task<IActionResult> GetMeasurementsByExperiment(Guid experimentId)
        => Ok(await _measurementService.GetByExperimentAsync(experimentId));

    [HttpPut("measurements/{id:guid}")]
    public async Task<IActionResult> UpdateMeasurement(Guid id, [FromBody] UpdateMeasurementDefinitionDto dto)
    {
        var result = await _measurementService.UpdateAsync(id, dto);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpDelete("measurements/{id:guid}")]
    public async Task<IActionResult> DeleteMeasurement(Guid id)
    {
        await _measurementService.DeleteAsync(id);
        return NoContent();
    }

    // ========== Procedure Templates ==========

    [HttpPost("procedure-templates")]
    public async Task<IActionResult> CreateProcedureTemplate([FromBody] CreateProcedureTemplateDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _templateService.CreateAsync(dto, GetUserId());
        return CreatedAtAction(nameof(GetProcedureTemplateById), new { id = result.Id }, result);
    }

    [HttpGet("procedure-templates")]
    public async Task<IActionResult> GetAllProcedureTemplates([FromQuery] Guid? cropVarietyId)
    {
        if (cropVarietyId.HasValue)
            return Ok(await _templateService.GetByCropVarietyAsync(cropVarietyId.Value));
        return Ok(await _templateService.GetAllAsync());
    }

    [HttpGet("procedure-templates/{id:guid}")]
    public async Task<IActionResult> GetProcedureTemplateById(Guid id)
    {
        var result = await _templateService.GetByIdAsync(id);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpDelete("procedure-templates/{id:guid}")]
    public async Task<IActionResult> DeleteProcedureTemplate(Guid id)
    {
        await _templateService.DeleteAsync(id);
        return NoContent();
    }

    // ========== Care Schedules ==========

    [HttpPost("{experimentId:guid}/schedules")]
    public async Task<IActionResult> CreateSchedule(Guid experimentId, [FromBody] CreateCareScheduleDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _scheduleService.CreateAsync(experimentId, dto);
        return CreatedAtAction(nameof(GetScheduleById), new { id = result.Id }, result);
    }

    [HttpGet("{experimentId:guid}/schedules")]
    public async Task<IActionResult> GetSchedulesByExperiment(Guid experimentId)
        => Ok(await _scheduleService.GetByExperimentAsync(experimentId));

    [HttpPut("schedules/{id:guid}")]
    public async Task<IActionResult> UpdateSchedule(Guid id, [FromBody] UpdateCareScheduleDto dto)
    {
        var result = await _scheduleService.UpdateAsync(id, dto);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpDelete("schedules/{id:guid}")]
    public async Task<IActionResult> DeleteSchedule(Guid id)
    {
        await _scheduleService.DeleteAsync(id);
        return NoContent();
    }

    private async Task<IActionResult> GetStageById(Guid id) => Ok(await _stageService.GetByExperimentAsync(Guid.Empty).ContinueWith(_ => (IActionResult)Ok()));
    private async Task<IActionResult> GetGroupById(Guid id) => Ok(await _groupService.GetByExperimentAsync(Guid.Empty).ContinueWith(_ => (IActionResult)Ok()));
    private async Task<IActionResult> GetMeasurementById(Guid id) => Ok(await _measurementService.GetByExperimentAsync(Guid.Empty).ContinueWith(_ => (IActionResult)Ok()));
    private async Task<IActionResult> GetScheduleById(Guid id) => Ok(await _scheduleService.GetByExperimentAsync(Guid.Empty).ContinueWith(_ => (IActionResult)Ok()));
}

public class UpdateStatusDto
{
    public string Status { get; set; } = string.Empty;
}
