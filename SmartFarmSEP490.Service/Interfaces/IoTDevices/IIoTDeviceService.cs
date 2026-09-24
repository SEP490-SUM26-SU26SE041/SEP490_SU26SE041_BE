using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SmartFarmSEP490.Model.DTOs;

namespace SmartFarmSEP490.Service.Interfaces.IoTDevices;

/// <summary>
/// Service xử lý nghiệp vụ IoTDevice cho Manager/Admin.
/// </summary>
public interface IIoTDeviceService
{
    /// <summary>Lấy tất cả IoTDevice</summary>
    Task<List<IoTDeviceResponseDto>> GetAllAsync();

    /// <summary>Lấy IoTDevice theo Id</summary>
    Task<IoTDeviceResponseDto?> GetByIdAsync(Guid id);

    /// <summary>Lấy IoTDevice theo DeviceCode</summary>
    Task<IoTDeviceResponseDto?> GetByDeviceCodeAsync(string deviceCode);

    /// <summary>Lấy IoTDevice theo BatchId</summary>
    Task<List<IoTDeviceResponseDto>> GetByBatchIdAsync(Guid batchId);

    /// <summary>Lấy danh sách thiết bị đang offline (cho Manager dashboard)</summary>
    Task<List<IoTDeviceListItemDto>> GetOfflineDevicesAsync();

    /// <summary>Tạo mới IoTDevice (kèm mapping Sensors)</summary>
    Task<IoTDeviceResponseDto> CreateAsync(CreateIoTDeviceDto dto);

    /// <summary>Cập nhật IoTDevice</summary>
    Task<IoTDeviceResponseDto> UpdateAsync(Guid id, UpdateIoTDeviceDto dto);

    /// <summary>Xóa IoTDevice</summary>
    Task<bool> DeleteAsync(Guid id);

    /// <summary>Gán thiết bị vào Batch (hoặc gỡ khỏi Batch nếu BatchId = null)</summary>
    Task<IoTDeviceResponseDto> AssignToBatchAsync(AssignDeviceToBatchDto dto);

    /// <summary>Bật/tắt IoT cho Batch</summary>
    Task<bool> ToggleBatchIoTAsync(ToggleBatchIoTDto dto);

    // ===== Sensor Data APIs =====

    /// <summary>Lấy lịch sử dữ liệu cảm biến của 1 thiết bị</summary>
    Task<IoTDeviceSensorHistoryResponseDto?> GetSensorDataAsync(
        Guid deviceId, DateTime? fromDate = null, DateTime? toDate = null, int limit = 100);

    /// <summary>Lấy giá trị cảm biến mới nhất của 1 thiết bị</summary>
    Task<List<IoTDeviceSensorDataDto>?> GetLatestSensorDataAsync(Guid deviceId);
}
