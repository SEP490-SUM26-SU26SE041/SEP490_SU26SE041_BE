using System;
using System.Collections.Generic;
using SmartFarmSEP490.Model.Enums;

namespace SmartFarmSEP490.Model.DTOs;

// =====================================================
// REQUEST DTOs (input)
// =====================================================

/// <summary>
/// DTO để map một Sensor với field MQTT tương ứng trên thiết bị.
/// Dùng khi tạo/cập nhật IoTDevice.
/// </summary>
public record IoTDeviceSensorDto(
    Guid? SensorId,           // Null nếu muốn hệ thống tự tạo Sensor mới
    string SensorCode,        // Mã cảm biến (ví dụ: "TEMP-AIR-ESP001-01")
    SensorType SensorType,    // Loại cảm biến
    string MqttFieldName      // Field trong JSON MQTT (ví dụ: "dht_h")
);

/// <summary>
/// Request tạo mới IoTDevice.
/// </summary>
public record CreateIoTDeviceDto(
    string DeviceCode,
    string DeviceName,
    string? MacAddress,
    string DeviceType,
    Guid? BatchId,
    bool IsActive,
    List<IoTDeviceSensorDto> Sensors
);

/// <summary>
/// Request cập nhật IoTDevice.
/// </summary>
public record UpdateIoTDeviceDto(
    string? DeviceName,
    string? MacAddress,
    Guid? BatchId,
    bool? IsActive,
    List<IoTDeviceSensorDto>? Sensors
);

/// <summary>
/// Request gán thiết bị vào Batch (hoặc gỡ khỏi Batch).
/// </summary>
public record AssignDeviceToBatchDto(
    Guid DeviceId,
    Guid? BatchId   // Null = gỡ khỏi Batch
);

/// <summary>
/// Request bật/tắt IoT cho Batch.
/// </summary>
public record ToggleBatchIoTDto(
    Guid BatchId,
    bool IsIoTEnabled
);

/// <summary>
/// Filter cho get danh sách IoTDevice.
/// </summary>
public record IoTDeviceFilterDto
{
    public Guid? BatchId { get; init; }
    public string? DeviceType { get; init; }
    public bool? IsActive { get; init; }
    public string? Search { get; init; }     // Tìm theo DeviceCode / DeviceName
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

// =====================================================
// RESPONSE DTOs (output)
// =====================================================

/// <summary>
/// Response chi tiết mapping Device-Sensor.
/// </summary>
public record IoTDeviceSensorResponseDto(
    Guid Id,
    Guid SensorId,
    string SensorCode,
    SensorType SensorType,
    string MqttFieldName
);

/// <summary>
/// Response chi tiết IoTDevice.
/// </summary>
public record IoTDeviceResponseDto(
    Guid Id,
    string DeviceCode,
    string DeviceName,
    string? MacAddress,
    string DeviceType,
    Guid? BatchId,
    string? BatchCode,
    bool IsActive,
    IoTDeviceStatus Status,
    bool IsOnline,                    // Tính từ LastActiveAt vs ngưỡng
    DateTime CreatedAt,
    DateTime UpdatedAt,
    DateTime? LastActiveAt,
    List<IoTDeviceSensorResponseDto> Sensors
);

/// <summary>
/// Response dạng danh sách (không bao gồm Sensors, dùng cho list view).
/// </summary>
public record IoTDeviceListItemDto(
    Guid Id,
    string DeviceCode,
    string DeviceName,
    string? MacAddress,
    string DeviceType,
    Guid? BatchId,
    string? BatchCode,
    bool IsActive,
    IoTDeviceStatus Status,
    bool IsOnline,
    DateTime? LastActiveAt
);

/// <summary>
/// Response dữ liệu cảm biến của IoTDevice.
/// </summary>
public record IoTDeviceSensorDataDto(
    Guid Id,
    Guid SensorId,
    string SensorCode,
    SensorType SensorType,
    decimal Value,
    DateTime RecordedAt
);

/// <summary>
/// Response lịch sử dữ liệu cảm biến của IoTDevice.
/// </summary>
public record IoTDeviceSensorHistoryResponseDto(
    Guid DeviceId,
    string DeviceCode,
    string? BatchCode,
    int TotalRecords,
    List<IoTDeviceSensorDataDto> SensorData
);
