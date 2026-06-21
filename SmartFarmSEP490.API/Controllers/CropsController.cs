using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartFarmSEP490.Model.DTOs;
using SmartFarmSEP490.Service.Interfaces.Resources;

namespace SmartFarmSEP490.API.Controllers;

[Route("api/crops")]
[ApiController]
[Authorize]
public class CropsController : ControllerBase
{
    private readonly ICropService _cropService;
    private readonly ICropVarietyService _varietyService;

    public CropsController(ICropService cropService, ICropVarietyService varietyService)
    {
        _cropService = cropService;
        _varietyService = varietyService;
    }

    // ========== Crops ==========

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> CreateCrop([FromBody] CreateCropDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _cropService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetCropById), new { id = result!.Id }, result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetCropById(Guid id)
    {
        var result = await _cropService.GetByIdAsync(id);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllCrops() => Ok(await _cropService.GetAllAsync());

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteCrop(Guid id)
    {
        await _cropService.DeleteAsync(id);
        return NoContent();
    }

    // ========== Crop Varieties ==========

    [Authorize(Roles = "Admin")]
    [HttpPost("varieties")]
    public async Task<IActionResult> CreateVariety([FromBody] CreateCropVarietyDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _varietyService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetVarietyById), new { id = result!.Id }, result);
    }

    [HttpGet("varieties/{id:guid}")]
    public async Task<IActionResult> GetVarietyById(Guid id)
    {
        var result = await _varietyService.GetByIdAsync(id);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpGet("crops/{cropId:guid}/varieties")]
    public async Task<IActionResult> GetVarietiesByCrop(Guid cropId)
        => Ok(await _varietyService.GetByCropAsync(cropId));

    [Authorize(Roles = "Admin")]
    [HttpDelete("varieties/{id:guid}")]
    public async Task<IActionResult> DeleteVariety(Guid id)
    {
        await _varietyService.DeleteAsync(id);
        return NoContent();
    }
}
