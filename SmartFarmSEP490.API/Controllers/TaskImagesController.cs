using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartFarmSEP490.Model.DTOs;
using SmartFarmSEP490.Service.Interfaces.Tasks;

namespace SmartFarmSEP490.API.Controllers;

[Route("api/task-images")]
[ApiController]
[Authorize]
public class TaskImagesController : ControllerBase
{
    private readonly ITaskImageService _imageService;

    public TaskImagesController(ITaskImageService imageService)
    {
        _imageService = imageService;
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)
        ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new UnauthorizedAccessException("User identifier claim not found."));

    /// <summary>
    /// Upload Task Image - dung cho Observation hoac Inspection
    /// </summary>
    [HttpPost("upload")]
    public async Task<IActionResult> UploadImage([FromBody] UploadTaskImageDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var result = await _imageService.UploadAsync(dto, GetUserId());
        return result == null ? BadRequest("Failed to upload image.") : Ok(result);
    }

    /// <summary>
    /// Get Images By Task (via TaskReportId)
    /// </summary>
    [HttpGet("task/{taskReportId:guid}")]
    public async Task<IActionResult> GetByTaskReport(Guid taskReportId)
    {
        return Ok(await _imageService.GetByTaskReportIdAsync(taskReportId));
    }

    /// <summary>
    /// Get Images By Batch
    /// </summary>
    [HttpGet("batch/{batchId:guid}")]
    public async Task<IActionResult> GetByBatch(Guid batchId)
    {
        return Ok(await _imageService.GetByBatchIdAsync(batchId));
    }

    /// <summary>
    /// Delete Image
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteImage(Guid id)
    {
        var deleted = await _imageService.DeleteAsync(id, GetUserId());
        return deleted ? NoContent() : NotFound();
    }
}
