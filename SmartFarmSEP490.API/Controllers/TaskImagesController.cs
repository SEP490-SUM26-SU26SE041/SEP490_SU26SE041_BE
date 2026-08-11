using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartFarmSEP490.API.Dtos;
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
    /// Upload Task Image - dùng cho Observation hoặc Inspection.
    /// FE gửi multipart/form-data gồm:
    ///   - file: IFormFile (required)
    ///   - experimentId: guid (required)
    ///   - batchId: guid? (optional)
    ///   - taskReportId: guid? (optional)
    ///   - caption: string? (optional)
    ///   - capturedAt: datetime? (optional, ISO 8601)
    /// File sẽ được push lên Cloudinary, response trả về imageUrl hosted.
    /// </summary>
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(20_000_000)] // 20 MB
    public async Task<IActionResult> UploadImage([FromForm] UploadTaskImageForm form, CancellationToken ct)
    {
        if (form == null || form.File == null || form.File.Length == 0)
            return BadRequest("File is required.");
        if (form.ExperimentId == Guid.Empty)
            return BadRequest("experimentId is required.");

        var result = await _imageService.UploadAsync(
            form.File,
            form.ExperimentId,
            form.BatchId,
            form.TaskReportId,
            form.Caption,
            form.CapturedAt,
            GetUserId(),
            ct);

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
