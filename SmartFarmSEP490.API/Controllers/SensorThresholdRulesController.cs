using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SmartFarmSEP490.Model.DTOs;
using SmartFarmSEP490.Model.Enums;
using SmartFarmSEP490.Service.Interfaces.SensorThresholds;

namespace SmartFarmSEP490.API.Controllers;

/// <summary>
/// API quản lý SensorThresholdRule (luật ngưỡng cảnh báo cảm biến).
/// </summary>
/// <remarks>
/// <para><b>Quyền truy cập:</b></para>
/// <list type="bullet">
/// <item><b>GET</b> (xem): <c>Researcher</c>, <c>Technician</c>, <c>Student</c> - xem được rule.</item>
/// <item><b>POST/PUT/PATCH/DELETE</b> (quản lý): chỉ <c>Researcher</c> - người chủ experiment tự cấu hình rule.</item>
/// </list>
/// <para>Rule có thể để <c>SensorType = null</c> để áp dụng cho mọi loại cảm biến.</para>
/// </remarks>
[Route("api/threshold-rules")]
[ApiController]
[Authorize(Roles = "Researcher")]
public class SensorThresholdRulesController : ControllerBase
{
    private readonly ISensorThresholdRuleService _service;
    private readonly ILogger<SensorThresholdRulesController> _logger;

    public SensorThresholdRulesController(
        ISensorThresholdRuleService service,
        ILogger<SensorThresholdRulesController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Lấy danh sách rules (hỗ trợ filter).
    /// <para><b>Quyền:</b> Researcher, Technician, Student (xem).</para>
    /// </summary>
    /// <param name="experimentId">Lọc theo experiment.</param>
    /// <param name="batchId">Lọc theo batch.</param>
    /// <param name="sensorType">Lọc theo loại cảm biến.</param>
    /// <param name="isActive">Lọc theo trạng thái active.</param>
    [HttpGet]
    [Authorize(Roles = "Researcher,Technician,Student")]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? experimentId,
        [FromQuery] Guid? batchId,
        [FromQuery] SensorType? sensorType,
        [FromQuery] bool? isActive)
    {
        var filter = new SensorThresholdRuleFilter
        {
            ExperimentId = experimentId,
            BatchId = batchId,
            SensorType = sensorType,
            IsActive = isActive
        };

        var result = await _service.GetAllAsync(filter);
        return Ok(new { success = true, count = result.Count, data = result });
    }

    /// <summary>
    /// Lấy chi tiết 1 rule.
    /// <para><b>Quyền:</b> Researcher, Technician, Student (xem).</para>
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Researcher,Technician,Student")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _service.GetByIdAsync(id);
        if (result == null)
            return NotFound(new { success = false, message = "Không tìm thấy rule." });
        return Ok(new { success = true, data = result });
    }

    /// <summary>
    /// Tìm rule áp dụng được cho 1 sensor cụ thể.
    /// <para><b>Quyền:</b> Researcher, Technician, Student (xem).</para>
    /// </summary>
    /// <param name="sensorType">Loại cảm biến.</param>
    /// <param name="batchId">Batch (có thể null).</param>
    /// <param name="experimentId">Experiment.</param>
    [HttpGet("applicable")]
    [Authorize(Roles = "Researcher,Technician,Student")]
    public async Task<IActionResult> FindApplicable(
        [FromQuery] SensorType sensorType,
        [FromQuery] Guid? batchId,
        [FromQuery] Guid experimentId)
    {
        var result = await _service.FindApplicableRuleAsync(sensorType, batchId, experimentId);
        if (result == null)
            return Ok(new { success = true, data = (SensorThresholdRuleDto?)null, message = "Không có rule áp dụng." });
        return Ok(new { success = true, data = result });
    }

    /// <summary>
    /// Tạo mới 1 rule (gắn với batch cụ thể — ExperimentId tự suy ra từ Batch).
    /// <para><b>Quyền:</b> chỉ <c>Researcher</c> (người cấu hình threshold cho experiment của mình).</para>
    /// </summary>
    /// <remarks>
    /// <para><b>Thay đổi:</b> từ phiên bản này, DTO tạo rule yêu cầu <c>batchId</c> bắt buộc
    /// (không còn <c>experimentId</c>). Mỗi rule phải gắn với 1 batch.</para>
    /// <para>Để tạo rule áp dụng cho toàn experiment (BatchId=null), dùng
    /// <c>POST /api/threshold-rules/experiment-wide</c>.</para>
    /// </remarks>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSensorThresholdRuleDto dto)
    {
        try
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.Id },
                new { success = true, data = result });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating SensorThresholdRule");
            return StatusCode(500, new { success = false, message = "Lỗi server." });
        }
    }

    /// <summary>
    /// Tạo rule áp dụng cho TOÀN experiment (BatchId=null — áp dụng cho mọi batch).
    /// <para><b>Quyền:</b> chỉ <c>Researcher</c>.</para>
    /// </summary>
    /// <remarks>
    /// <para><b>Khi nào dùng endpoint này?</b></para>
    /// <para>Khi bạn muốn một quy tắc <b>CHUNG cho mọi batch</b> trong cùng experiment. Ví dụ:</para>
    /// <list type="bullet">
    /// <item>Sensor trả về 0 (bất kể batch nào) → cảnh báo offline.</item>
    /// <item>pH > 8 (bất kể giống cây) → nguy hiểm chung.</item>
    /// <item>Nhiệt độ > 40°C (bất kể giai đoạn) → Critical.</item>
    /// </list>
    /// <para><b>So với endpoint chính:</b></para>
    /// <list type="bullet">
    /// <item><c>POST /api/threshold-rules</c> → gắn 1 batch cụ thể (BatchId bắt buộc).</item>
    /// <item><c>POST /api/threshold-rules/experiment-wide</c> → gắn cả experiment (BatchId=null).</item>
    /// </list>
    /// <para><b>Ưu điểm:</b> chỉ tạo 1 rule thay vì tạo N rule cho N batch. Batch mới tạo sau sẽ tự động áp dụng.</para>
    /// </remarks>
    [HttpPost("experiment-wide")]
    public async Task<IActionResult> CreateExperimentWide([FromBody] CreateExperimentWideRuleDto dto)
    {
        try
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _service.CreateExperimentWideAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.Id },
                new { success = true, data = result });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating ExperimentWide SensorThresholdRule");
            return StatusCode(500, new { success = false, message = "Lỗi server." });
        }
    }

    /// <summary>
    /// Cập nhật 1 rule (partial update).
    /// <para><b>Quyền:</b> chỉ <c>Researcher</c>.</para>
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSensorThresholdRuleDto dto)
    {
        try
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _service.UpdateAsync(id, dto);
            return Ok(new { success = true, data = result });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating SensorThresholdRule {Id}", id);
            return StatusCode(500, new { success = false, message = "Lỗi server." });
        }
    }

    /// <summary>
    /// Bật/tắt nhanh 1 rule.
    /// <para><b>Quyền:</b> chỉ <c>Researcher</c>.</para>
    /// </summary>
    [HttpPatch("{id:guid}/toggle")]
    public async Task<IActionResult> Toggle(Guid id, [FromBody] bool isActive)
    {
        try
        {
            var result = await _service.ToggleActiveAsync(id, isActive);
            return Ok(new { success = true, data = result });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling SensorThresholdRule {Id}", id);
            return StatusCode(500, new { success = false, message = "Lỗi server." });
        }
    }

    /// <summary>
    /// Xóa 1 rule.
    /// <para><b>Quyền:</b> chỉ <c>Researcher</c>.</para>
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _service.DeleteAsync(id);
        if (!result)
            return NotFound(new { success = false, message = "Không tìm thấy rule." });
        return Ok(new { success = true, message = "Đã xóa rule." });
    }
}