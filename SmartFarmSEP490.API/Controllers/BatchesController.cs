using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartFarmSEP490.Model.DTOs;
using SmartFarmSEP490.Repository.Interfaces.Experiments;
using SmartFarmSEP490.Service.Interfaces.Resources;

namespace SmartFarmSEP490.API.Controllers;

[Route("api/batches")]
[ApiController]
[Authorize]
public class BatchesController : ControllerBase
{
    private readonly IBatchService _batchService;
    private readonly IExperimentRepository _experimentRepository;

    public BatchesController(
        IBatchService batchService,
        IExperimentRepository experimentRepository)
    {
        _batchService = batchService;
        _experimentRepository = experimentRepository;
    }

    private Guid GetUserId() =>
        Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : Guid.Empty;

    private string? GetRole() => User.FindFirstValue(ClaimTypes.Role);

    private bool IsResearcher() => GetRole() == "Researcher";

    private bool IsManager() => GetRole() == "Manager";

    private async Task<bool> IsExperimentOwnerAsync(Guid experimentId)
    {
        var exp = await _experimentRepository.GetByIdAsync(experimentId);
        if (exp == null) return false;
        return exp.ResearcherId == GetUserId();
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBatchDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        if (!IsResearcher()) return Forbid();
        if (!await IsExperimentOwnerAsync(dto.ExperimentId)) return Forbid();
        try
        {
            var result = await _batchService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _batchService.GetByIdAsync(id);
        if (result == null) return NotFound();
        if (IsManager()) return Forbid();
        if (!await IsExperimentOwnerAsync(result.ExperimentId)) return Forbid();
        return Ok(result);
    }

    [HttpGet("experiments/{experimentId:guid}")]
    public async Task<IActionResult> GetByExperiment(Guid experimentId)
    {
        if (IsManager()) return Ok(new List<BatchResponseDto>());
        if (!IsResearcher()) return Forbid();
        if (!await IsExperimentOwnerAsync(experimentId)) return Forbid();
        return Ok(await _batchService.GetByExperimentAsync(experimentId));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBatchDto dto)
    {
        var existing = await _batchService.GetByIdAsync(id);
        if (existing == null) return NotFound();
        if (!IsResearcher()) return Forbid();
        if (!await IsExperimentOwnerAsync(existing.ExperimentId)) return Forbid();
        var result = await _batchService.UpdateAsync(id, dto);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var existing = await _batchService.GetByIdAsync(id);
        if (existing == null) return NotFound();
        if (!IsResearcher()) return Forbid();
        if (!await IsExperimentOwnerAsync(existing.ExperimentId)) return Forbid();
        await _batchService.DeleteAsync(id);
        return NoContent();
    }
}