using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartFarmSEP490.Model.DTOs;
using SmartFarmSEP490.Service.Interfaces.AI;
using SmartFarmSEP490.Service.Interfaces.Tasks;

namespace SmartFarmSEP490.API.Controllers;

[Route("api/task-images")]
[ApiController]
[Authorize]
public class TaskImagesController : ControllerBase
{
    private readonly ITaskImageService _imageService;
    private readonly IAIAnalysisService _aiService;

    public TaskImagesController(ITaskImageService imageService, IAIAnalysisService aiService)
    {
        _imageService = imageService;
        _aiService = aiService;
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
    ///   - aiProvider: "TomatoLeafDiseaseOnnx" | "ArgoPestOnnx" (optional - tự suy ra từ caption)
    /// File sẽ được push lên Cloudinary, response trả về imageUrl hosted + AIStatus=Pending.
    /// AI chạy async trong Background Worker; FE poll GET /api/task-images/task/{taskReportId}/detail để lấy kết quả.
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
            form.AIProvider,  // nullable — Worker tự suy ra nếu null
            GetUserId(),
            ct);

        return result == null ? BadRequest("Failed to upload image.") : Ok(result);
    }

    /// <summary>
    /// Get Images By Task (qua TaskReportId) — bản cũ trả DTO đơn giản.
    /// </summary>
    [HttpGet("task/{taskReportId:guid}")]
    public async Task<IActionResult> GetByTaskReport(Guid taskReportId)
    {
        return Ok(await _imageService.GetByTaskReportIdAsync(taskReportId));
    }

    /// <summary>
    /// Get Images By Task — bản chi tiết có AIAnalysis đầy đủ (cho FE poll).
    /// </summary>
    /// <param name="includeAnalysis">Mặc định true. Set false nếu chỉ cần list + AIStatus.</param>
    [HttpGet("task/{taskReportId:guid}/detail")]
    public async Task<IActionResult> GetByTaskReportDetail(Guid taskReportId, [FromQuery] bool includeAnalysis = true, CancellationToken ct = default)
    {
        var result = await _aiService.GetByTaskReportAsync(taskReportId, includeAnalysis, ct);
        return Ok(result);
    }

    /// <summary>
    /// Re-enqueue một PlantImage để AI xử lý lại (dùng khi Failed hoặc muốn predict lại với provider khác).
    /// </summary>
    [HttpPost("{id:guid}/retry")]
    public async Task<IActionResult> Retry(Guid id, CancellationToken ct)
    {
        await _aiService.EnqueueAsync(id, ct);
        return Accepted(new { id, message = "Re-enqueued for AI processing." });
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
