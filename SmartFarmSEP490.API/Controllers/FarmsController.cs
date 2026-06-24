using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartFarmSEP490.API.Helpers;
using SmartFarmSEP490.Model.DTOs;
using SmartFarmSEP490.Service.Interfaces.Resources;

namespace SmartFarmSEP490.API.Controllers;

[Route("api/farms")]
[ApiController]
[Authorize(Roles = "Manager")]
public class FarmsController : ControllerBase
{
    private readonly IFarmService _farmService;
    private readonly IAreaService _areaService;
    private readonly IBedService _bedService;
    private readonly IExperimentBedAssignmentService _assignmentService;

    public FarmsController(
        IFarmService farmService,
        IAreaService areaService,
        IBedService bedService,
        IExperimentBedAssignmentService assignmentService)
    {
        _farmService = farmService;
        _areaService = areaService;
        _bedService = bedService;
        _assignmentService = assignmentService;
    }

    // ========== Farms ==========

    [HttpPost]
    public async Task<IActionResult> CreateFarm([FromBody] CreateFarmDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var userId = this.GetUserId();
        var result = await _farmService.CreateAsync(dto, userId == Guid.Empty ? null : userId);
        if (result == null) return StatusCode(500, new { message = "Tao nong trai that bai." });
        return CreatedAtAction(nameof(GetFarmById), new { id = result.Id }, result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetFarmById(Guid id)
    {
        var result = await _farmService.GetByIdAsync(id);
        if (result == null) return NotFound();
        if (!await CanAccessFarmAsync(id)) return Forbid();
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllFarms() => Ok(await _farmService.GetAllAsync());

    [HttpGet("my-farms")]
    public async Task<IActionResult> GetMyFarms()
    {
        var userId = this.GetUserId();
        if (userId == Guid.Empty) return Unauthorized();
        return Ok(await _farmService.GetByManagerAsync(userId));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateFarm(Guid id, [FromBody] UpdateFarmDto dto)
    {
        if (!await CanAccessFarmAsync(id)) return Forbid();
        var result = await _farmService.UpdateAsync(id, dto);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPost("{farmId:guid}/manager/{managerId:guid}")]
    public async Task<IActionResult> AssignManager(Guid farmId, Guid managerId)
    {
        if (!await CanAccessFarmAsync(farmId)) return Forbid();
        var result = await _farmService.AssignManagerAsync(farmId, managerId);
        return result ? Ok(new { farmId, managerId }) : NotFound();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteFarm(Guid id)
    {
        if (!await CanAccessFarmAsync(id)) return Forbid();
        await _farmService.DeleteAsync(id);
        return NoContent();
    }

    // ========== Areas ==========

    [HttpPost("areas")]
    public async Task<IActionResult> CreateArea([FromBody] CreateAreaDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        if (!await CanAccessFarmAsync(dto.FarmId)) return Forbid();
        var result = await _areaService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetAreaById), new { id = result.Id }, result);
    }

    [HttpGet("areas/{id:guid}")]
    public async Task<IActionResult> GetAreaById(Guid id)
    {
        var result = await _areaService.GetByIdAsync(id);
        if (result == null) return NotFound();
        if (!await CanAccessFarmAsync(result.FarmId)) return Forbid();
        return Ok(result);
    }

    [HttpGet("farms/{farmId:guid}/areas")]
    public async Task<IActionResult> GetAreasByFarm(Guid farmId)
    {
        if (!await CanAccessFarmAsync(farmId)) return Forbid();
        return Ok(await _areaService.GetByFarmAsync(farmId));
    }

    [HttpPut("areas/{id:guid}")]
    public async Task<IActionResult> UpdateArea(Guid id, [FromBody] UpdateAreaDto dto)
    {
        var existing = await _areaService.GetByIdAsync(id);
        if (existing == null) return NotFound();
        if (!await CanAccessFarmAsync(existing.FarmId)) return Forbid();
        var result = await _areaService.UpdateAsync(id, dto);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpDelete("areas/{id:guid}")]
    public async Task<IActionResult> DeleteArea(Guid id)
    {
        var existing = await _areaService.GetByIdAsync(id);
        if (existing == null) return NotFound();
        if (!await CanAccessFarmAsync(existing.FarmId)) return Forbid();
        await _areaService.DeleteAsync(id);
        return NoContent();
    }

    // ========== Beds ==========

    [HttpPost("beds")]
    public async Task<IActionResult> CreateBed([FromBody] CreateBedDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var area = await _areaService.GetByIdAsync(dto.AreaId);
        if (area == null) return BadRequest(new { message = "Khu vuc khong ton tai." });
        if (!await CanAccessFarmAsync(area.FarmId)) return Forbid();
        var result = await _bedService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetBedById), new { id = result.Id }, result);
    }

    [HttpGet("beds/{id:guid}")]
    public async Task<IActionResult> GetBedById(Guid id)
    {
        var result = await _bedService.GetByIdAsync(id);
        if (result == null) return NotFound();
        if (!await CanAccessFarmAsync(result.FarmId)) return Forbid();
        return Ok(result);
    }

    [HttpGet("areas/{areaId:guid}/beds")]
    public async Task<IActionResult> GetBedsByArea(Guid areaId)
    {
        var area = await _areaService.GetByIdAsync(areaId);
        if (area == null) return NotFound();
        if (!await CanAccessFarmAsync(area.FarmId)) return Forbid();
        return Ok(await _bedService.GetByAreaAsync(areaId));
    }

    [HttpGet("farms/{farmId:guid}/beds/available")]
    public async Task<IActionResult> GetAvailableBedsByFarm(Guid farmId)
    {
        if (!await CanAccessFarmAsync(farmId)) return Forbid();
        return Ok(await _bedService.GetAvailableByFarmAsync(farmId));
    }

    [HttpPut("beds/{id:guid}")]
    public async Task<IActionResult> UpdateBed(Guid id, [FromBody] UpdateBedDto dto)
    {
        var existing = await _bedService.GetByIdAsync(id);
        if (existing == null) return NotFound();
        if (!await CanAccessFarmAsync(existing.FarmId)) return Forbid();
        var result = await _bedService.UpdateAsync(id, dto);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpDelete("beds/{id:guid}")]
    public async Task<IActionResult> DeleteBed(Guid id)
    {
        var existing = await _bedService.GetByIdAsync(id);
        if (existing == null) return NotFound();
        if (!await CanAccessFarmAsync(existing.FarmId)) return Forbid();
        await _bedService.DeleteAsync(id);
        return NoContent();
    }

    // ========== Experiment Bed Assignments ==========

    [HttpPost("bed-assignments")]
    public async Task<IActionResult> CreateBedAssignment([FromBody] CreateExperimentBedAssignmentDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var bed = await _bedService.GetByIdAsync(dto.BedId);
        if (bed == null) return BadRequest(new { message = "Lo trong khong ton tai." });
        if (!await CanAccessFarmAsync(bed.FarmId)) return Forbid();
        try
        {
            var result = await _assignmentService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetBedAssignmentById), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpGet("bed-assignments/{id:guid}")]
    public async Task<IActionResult> GetBedAssignmentById(Guid id)
    {
        var assignments = await _assignmentService.GetByExperimentAsync(id);
        var first = assignments.FirstOrDefault(a => a.Id == id);
        if (first == null) return NotFound();
        var bed = await _bedService.GetByIdAsync(first.BedId);
        if (bed == null || !await CanAccessFarmAsync(bed.FarmId)) return Forbid();
        return Ok(first);
    }

    [HttpGet("experiments/{experimentId:guid}/bed-assignments")]
    public async Task<IActionResult> GetBedAssignmentsByExperiment(Guid experimentId)
    {
        var assignments = await _assignmentService.GetByExperimentAsync(experimentId);
        var filtered = new List<ExperimentBedAssignmentResponseDto>();
        foreach (var a in assignments)
        {
            var bed = await _bedService.GetByIdAsync(a.BedId);
            if (bed != null && await CanAccessFarmAsync(bed.FarmId)) filtered.Add(a);
        }
        return Ok(filtered);
    }

    [HttpPut("bed-assignments/{id:guid}")]
    public async Task<IActionResult> UpdateBedAssignment(Guid id, [FromBody] UpdateExperimentBedAssignmentDto dto)
    {
        var all = await _assignmentService.GetByBedAsync(id);
        var existing = all.FirstOrDefault(a => a.Id == id);
        if (existing == null) return NotFound();
        var bed = await _bedService.GetByIdAsync(existing.BedId);
        if (bed == null || !await CanAccessFarmAsync(bed.FarmId)) return Forbid();
        var result = await _assignmentService.UpdateAsync(id, dto);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpDelete("bed-assignments/{id:guid}")]
    public async Task<IActionResult> DeleteBedAssignment(Guid id)
    {
        var all = await _assignmentService.GetByBedAsync(id);
        var existing = all.FirstOrDefault(a => a.Id == id);
        if (existing == null) return NotFound();
        var bed = await _bedService.GetByIdAsync(existing.BedId);
        if (bed == null || !await CanAccessFarmAsync(bed.FarmId)) return Forbid();
        await _assignmentService.DeleteAsync(id);
        return NoContent();
    }

    // ========== Authorization Helper ==========

    private async Task<bool> CanAccessFarmAsync(Guid farmId)
    {
        var userId = this.GetUserId();
        if (userId == Guid.Empty) return false;
        var farm = await _farmService.GetByIdAsync(farmId);
        if (farm == null) return false;
        if (farm.ManagerId == null)
        {
            Console.WriteLine($"[ACCESS] Farm {farmId} has no ManagerId. Assigning creator {userId} as manager.");
            await _farmService.AssignManagerAsync(farmId, userId);
            return true;
        }
        if (farm.ManagerId == userId) return true;
        if (this.GetUserRole() == "Admin") return true;
        return false;
    }
}
