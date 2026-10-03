using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SmartFarmSEP490.Model.DTOs;
using SmartFarmSEP490.Service.Interfaces.IoTDevices;

namespace SmartFarmSEP490.API.Controllers;

/// <summary>
/// API quản lý thiết bị IoT (ESP32-C3 Water Sensor).
/// Quyền: Researcher, Technician, Student — full quyền (xem + tạo + sửa + xóa + gán batch + toggle IoT).
/// </summary>
[Route("api/iot-devices")]
[ApiController]
[Authorize(Roles = "Researcher,Technician,Student")]
public class IoTDevicesController : ControllerBase
{
    private readonly IIoTDeviceService _service;
    private readonly ILogger<IoTDevicesController> _logger;

    public IoTDevicesController(IIoTDeviceService service, ILogger<IoTDevicesController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Lấy danh sách tất cả thiết bị IoT.
    /// <para><b>Quyền:</b> Researcher, Technician, Student.</para>
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetAllAsync();
        return Ok(new { success = true, data = result });
    }

    /// <summary>
    /// Lấy danh sách thiết bị đang offline.
    /// <para><b>Quyền:</b> Researcher, Technician, Student.</para>
    /// </summary>
    [HttpGet("offline")]
    public async Task<IActionResult> GetOfflineDevices()
    {
        var result = await _service.GetOfflineDevicesAsync();
        return Ok(new { success = true, data = result });
    }

    /// <summary>
    /// Lấy chi tiết 1 thiết bị.
    /// <para><b>Quyền:</b> Researcher, Technician, Student.</para>
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _service.GetByIdAsync(id);
        if (result == null)
            return NotFound(new { success = false, message = "Không tìm thấy thiết bị." });
        return Ok(new { success = true, data = result });
    }

    /// <summary>
    /// Lấy thiết bị theo DeviceCode (dùng cho MQTT auto-discovery).
    /// <para><b>Quyền:</b> Researcher, Technician, Student.</para>
    /// </summary>
    [HttpGet("code/{deviceCode}")]
    public async Task<IActionResult> GetByDeviceCode(string deviceCode)
    {
        var result = await _service.GetByDeviceCodeAsync(deviceCode);
        if (result == null)
            return NotFound(new { success = false, message = "Không tìm thấy thiết bị." });
        return Ok(new { success = true, data = result });
    }

    /// <summary>
    /// Lấy danh sách thiết bị theo BatchId.
    /// <para><b>Quyền:</b> Researcher, Technician, Student.</para>
    /// </summary>
    [HttpGet("batch/{batchId:guid}")]
    public async Task<IActionResult> GetByBatch(Guid batchId)
    {
        var result = await _service.GetByBatchIdAsync(batchId);
        return Ok(new { success = true, data = result });
    }

    /// <summary>
    /// Tạo mới thiết bị IoT.
    /// <para><b>Quyền:</b> Researcher, Technician, Student.</para>
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateIoTDeviceDto dto)
    {
        try
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.Id },
                new { success = true, data = result });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating IoTDevice");
            return StatusCode(500, new { success = false, message = "Lỗi server." });
        }
    }

    /// <summary>
    /// Cập nhật thiết bị IoT.
    /// <para><b>Quyền:</b> Researcher, Technician, Student.</para>
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateIoTDeviceDto dto)
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
            _logger.LogError(ex, "Error updating IoTDevice {Id}", id);
            return StatusCode(500, new { success = false, message = "Lỗi server." });
        }
    }

    /// <summary>
    /// Xóa thiết bị IoT.
    /// <para><b>Quyền:</b> Researcher, Technician, Student.</para>
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _service.DeleteAsync(id);
        if (!result)
            return NotFound(new { success = false, message = "Không tìm thấy thiết bị." });
        return Ok(new { success = true, message = "Đã xóa thiết bị." });
    }

    /// <summary>
    /// Gán thiết bị vào Batch (hoặc gỡ nếu BatchId = null).
    /// <para><b>Quyền:</b> Researcher, Technician, Student.</para>
    /// </summary>
    [HttpPost("assign-batch")]
    public async Task<IActionResult> AssignToBatch([FromBody] AssignDeviceToBatchDto dto)
    {
        try
        {
            var result = await _service.AssignToBatchAsync(dto);
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
            _logger.LogError(ex, "Error assigning IoTDevice to batch");
            return StatusCode(500, new { success = false, message = "Lỗi server." });
        }
    }

    /// <summary>
    /// Bật/tắt IoT cho một Batch.
    /// <para><b>Quyền:</b> Researcher, Technician, Student.</para>
    /// </summary>
    [HttpPost("batch/toggle-iot")]
    public async Task<IActionResult> ToggleBatchIoT([FromBody] ToggleBatchIoTDto dto)
    {
        try
        {
            var result = await _service.ToggleBatchIoTAsync(dto);
            return Ok(new { success = true, isIoTEnabled = result, message = result ? "Đã bật IoT cho batch." : "Đã tắt IoT cho batch." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling batch IoT");
            return StatusCode(500, new { success = false, message = "Lỗi server." });
        }
    }

    // ============== SENSOR DATA APIs ==============

    /// <summary>
    /// Lấy lịch sử dữ liệu cảm biến của 1 thiết bị.
    /// <para><b>Quyền:</b> Researcher, Technician, Student.</para>
    /// </summary>
    [HttpGet("{id:guid}/sensor-data")]
    public async Task<IActionResult> GetSensorData(
        Guid id,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] int limit = 100)
    {
        var result = await _service.GetSensorDataAsync(id, fromDate, toDate, limit);
        if (result == null)
            return NotFound(new { success = false, message = "Không tìm thấy thiết bị." });
        return Ok(new { success = true, data = result });
    }

    /// <summary>
    /// Lấy giá trị cảm biến mới nhất của 1 thiết bị.
    /// <para><b>Quyền:</b> Researcher, Technician, Student.</para>
    /// </summary>
    [HttpGet("{id:guid}/sensor-data/latest")]
    public async Task<IActionResult> GetLatestSensorData(Guid id)
    {
        var result = await _service.GetLatestSensorDataAsync(id);
        if (result == null)
            return NotFound(new { success = false, message = "Không tìm thấy thiết bị." });
        return Ok(new { success = true, data = result });
    }
}