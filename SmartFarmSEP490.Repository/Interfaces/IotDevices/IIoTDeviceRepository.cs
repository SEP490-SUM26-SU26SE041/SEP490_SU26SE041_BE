using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SmartFarmSEP490.Model;
using Task = System.Threading.Tasks.Task;

namespace SmartFarmSEP490.Repository.Interfaces.IoTDevices;

/// <summary>
/// Repository cho IoTDevice và IoTDeviceSensor.
/// </summary>
public interface IIoTDeviceRepository
{
    // ============= IoTDevice =============

    /// <summary>Lấy IoTDevice theo Id (kèm DeviceSensors + Sensor + Batch)</summary>
    Task<IoTDevice?> GetByIdAsync(Guid id);

    /// <summary>Lấy IoTDevice theo DeviceCode</summary>
    Task<IoTDevice?> GetByDeviceCodeAsync(string deviceCode);

    /// <summary>Lấy tất cả IoTDevice (kèm navigation đầy đủ)</summary>
    Task<List<IoTDevice>> GetAllAsync();

    /// <summary>Lấy IoTDevice theo BatchId</summary>
    Task<List<IoTDevice>> GetByBatchIdAsync(Guid batchId);

    /// <summary>Lấy danh sách thiết bị đang Active và đã được gán Batch (cho health monitor)</summary>
    Task<List<IoTDevice>> GetActiveDevicesAsync();

    /// <summary>Thêm mới IoTDevice</summary>
    Task AddAsync(IoTDevice device);

    /// <summary>Cập nhật IoTDevice</summary>
    Task UpdateAsync(IoTDevice device);

    /// <summary>Xóa IoTDevice</summary>
    Task DeleteAsync(IoTDevice device);

    /// <summary>Save changes</summary>
    Task SaveChangesAsync();

    // ============= IoTDeviceSensor =============

    /// <summary>Lấy danh sách Sensor mappings của 1 device</summary>
    Task<List<IoTDeviceSensor>> GetDeviceSensorsAsync(Guid deviceId);

    /// <summary>Lấy mapping theo Id</summary>
    Task<IoTDeviceSensor?> GetDeviceSensorByIdAsync(Guid id);

    /// <summary>Tìm mapping theo DeviceId + MqttFieldName</summary>
    Task<IoTDeviceSensor?> GetByDeviceAndMqttFieldAsync(Guid deviceId, string mqttFieldName);

    /// <summary>Thêm mapping mới</summary>
    Task AddDeviceSensorAsync(IoTDeviceSensor mapping);

    /// <summary>Thêm nhiều mappings</summary>
    Task AddDeviceSensorsAsync(IEnumerable<IoTDeviceSensor> mappings);

    /// <summary>Xóa tất cả mappings của 1 device</summary>
    Task DeleteDeviceSensorsByDeviceIdAsync(Guid deviceId);
}
