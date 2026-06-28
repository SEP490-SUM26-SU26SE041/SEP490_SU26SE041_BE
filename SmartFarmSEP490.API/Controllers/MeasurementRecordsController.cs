using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartFarmSEP490.Model.DTOs;
using SmartFarmSEP490.Service.Interfaces.Tasks;

namespace SmartFarmSEP490.API.Controllers;

[Route("api/measurement-records")]
[ApiController]
[Authorize]
public class MeasurementRecordsController : ControllerBase
{
    private readonly IMeasurementRecordService _recordService;

    public MeasurementRecordsController(IMeasurementRecordService recordService)
    {
        _recordService = recordService;
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)
        ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new UnauthorizedAccessException("User identifier claim not found."));

    /// <summary>
    /// Create Measurement Record
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateRecord([FromBody] CreateMeasurementRecordDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var result = await _recordService.CreateAsync(dto, GetUserId());
        return result == null ? BadRequest("Failed to create record.") : CreatedAtAction(nameof(GetHistory), new { batchId = dto.BatchId }, result);
    }

    /// <summary>
    /// Update Measurement Record
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateRecord(Guid id, [FromBody] UpdateMeasurementRecordDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var result = await _recordService.UpdateAsync(id, dto, GetUserId());
        return result == null ? NotFound() : Ok(result);
    }

    /// <summary>
    /// Delete Measurement Record
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteRecord(Guid id)
    {
        var deleted = await _recordService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }

    /// <summary>
    /// Get Measurement History By Batch
    /// </summary>
    [HttpGet("batch/{batchId:guid}")]
    public async Task<IActionResult> GetHistory(Guid batchId)
    {
        return Ok(await _recordService.GetByBatchIdAsync(batchId));
    }
}
