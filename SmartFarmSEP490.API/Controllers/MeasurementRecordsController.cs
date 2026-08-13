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
    /// Create a single measurement record
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateRecord([FromBody] CreateMeasurementRecordDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var result = await _recordService.CreateAsync(dto, GetUserId());
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Create many measurement records for one batch at the same time
    /// (use case: technician measures 1 batch with N metrics → submit all N records in 1 request)
    /// </summary>
    [HttpPost("bulk")]
    public async Task<IActionResult> CreateBulk([FromBody] BulkCreateMeasurementRecordDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var result = await _recordService.CreateBulkAsync(dto, GetUserId());
            return result.Created == 0
                ? BadRequest(new { success = false, message = "Không tạo được bản ghi nào.", data = result })
                : Ok(new { success = true, message = $"Tạo {result.Created} bản ghi, bỏ qua {result.Skipped}.", data = result });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Update Measurement Record
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateRecord(Guid id, [FromBody] UpdateMeasurementRecordDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var result = await _recordService.UpdateAsync(id, dto, GetUserId());
        return Ok(result);
    }

    /// <summary>
    /// Delete Measurement Record (Soft Delete)
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteRecord(Guid id)
    {
        await _recordService.DeleteAsync(id);
        return NoContent();
    }

    /// <summary>
    /// Get Measurement Record By Id
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _recordService.GetByIdAsync(id);
        return Ok(result);
    }

    /// <summary>
    /// Get Measurement History By Batch
    /// </summary>
    [HttpGet("batch/{batchId:guid}")]
    public async Task<IActionResult> GetByBatch(Guid batchId)
    {
        return Ok(await _recordService.GetByBatchIdAsync(batchId));
    }

    /// <summary>
    /// Get Measurement Records By Experiment
    /// </summary>
    [HttpGet("experiment/{experimentId:guid}")]
    public async Task<IActionResult> GetByExperiment(Guid experimentId)
    {
        return Ok(await _recordService.GetByExperimentIdAsync(experimentId));
    }

    /// <summary>
    /// Get Measurement Records By Stage
    /// </summary>
    [HttpGet("stage/{stageId:guid}")]
    public async Task<IActionResult> GetByStage(Guid stageId)
    {
        return Ok(await _recordService.GetByStageIdAsync(stageId));
    }
}
