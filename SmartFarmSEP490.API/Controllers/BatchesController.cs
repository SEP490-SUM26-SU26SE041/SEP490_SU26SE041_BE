using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartFarmSEP490.API.Helpers;
using SmartFarmSEP490.Model.DTOs;
using SmartFarmSEP490.Service.Interfaces.Resources;

namespace SmartFarmSEP490.API.Controllers;

[Route("api/batches")]
[ApiController]
[Authorize(Roles = "Manager")]
public class BatchesController : ControllerBase
{
    private readonly IBatchService _batchService;
    private readonly IFarmService _farmService;
    private readonly IBedService _bedService;
    private readonly IExperimentBedAssignmentService _assignmentService;

    public BatchesController(
        IBatchService batchService,
        IFarmService farmService,
        IBedService bedService,
        IExperimentBedAssignmentService assignmentService)
    {
        _batchService = batchService;
        _farmService = farmService;
        _bedService = bedService;
        _assignmentService = assignmentService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBatchDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        if (!await CanAccessBatchFarmAsync(dto)) return Forbid();
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
        if (!await CanAccessBatchAsync(result)) return Forbid();
        return Ok(result);
    }

    [HttpGet("experiments/{experimentId:guid}")]
    public async Task<IActionResult> GetByExperiment(Guid experimentId)
    {
        var all = await _batchService.GetByExperimentAsync(experimentId);
        var userId = this.GetUserId();
        if (userId == Guid.Empty) return Unauthorized();
        var myFarms = (await _farmService.GetByManagerAsync(userId)).Select(f => f.Id).ToHashSet();
        var filtered = new List<BatchResponseDto>();
        foreach (var b in all)
        {
            var farmId = await ResolveFarmIdForBatchAsync(b);
            if (farmId.HasValue && myFarms.Contains(farmId.Value)) filtered.Add(b);
        }
        return Ok(filtered);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBatchDto dto)
    {
        var existing = await _batchService.GetByIdAsync(id);
        if (existing == null) return NotFound();
        if (!await CanAccessBatchAsync(existing)) return Forbid();
        var result = await _batchService.UpdateAsync(id, dto);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var existing = await _batchService.GetByIdAsync(id);
        if (existing == null) return NotFound();
        if (!await CanAccessBatchAsync(existing)) return Forbid();
        await _batchService.DeleteAsync(id);
        return NoContent();
    }

    // Resolve the FarmId that owns a batch through its ExperimentBedAssignment → Bed → Area → Farm
    private async Task<Guid?> ResolveFarmIdForBatchAsync(BatchResponseDto batch)
    {
        if (batch.ExperimentBedAssignmentId.HasValue)
        {
            var assignments = await _assignmentService.GetByExperimentAsync(batch.ExperimentId);
            var a = assignments.FirstOrDefault(x => x.Id == batch.ExperimentBedAssignmentId.Value);
            if (a != null)
            {
                var bed = await _bedService.GetByIdAsync(a.BedId);
                if (bed != null) return bed.FarmId;
            }
        }
        return null;
    }

    private async Task<bool> CanAccessBatchAsync(BatchResponseDto batch)
    {
        var farmId = await ResolveFarmIdForBatchAsync(batch);
        if (farmId == null) return false;
        return await CanAccessFarmInternalAsync(farmId.Value);
    }

    private async Task<bool> CanAccessBatchFarmAsync(CreateBatchDto dto)
    {
        if (dto.ExperimentBedAssignmentId.HasValue)
        {
            var assignments = await _assignmentService.GetByExperimentAsync(dto.ExperimentId);
            var a = assignments.FirstOrDefault(x => x.Id == dto.ExperimentBedAssignmentId.Value);
            if (a != null)
            {
                var bed = await _bedService.GetByIdAsync(a.BedId);
                if (bed != null) return await CanAccessFarmInternalAsync(bed.FarmId);
            }
            return false;
        }
        // No assignment: allow creation if the manager owns at least one farm.
        var userId = this.GetUserId();
        if (userId == Guid.Empty) return false;
        var farms = await _farmService.GetByManagerAsync(userId);
        return farms.Count > 0;
    }

    private async Task<bool> CanAccessFarmInternalAsync(Guid farmId)
    {
        var userId = this.GetUserId();
        if (userId == Guid.Empty) return false;
        var farm = await _farmService.GetByIdAsync(farmId);
        return farm != null && farm.ManagerId == userId;
    }
}
