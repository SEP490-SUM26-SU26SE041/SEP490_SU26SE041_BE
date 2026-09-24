using System;
using System.Collections.Generic;
using SmartFarmSEP490.Model.Enums;

namespace SmartFarmSEP490.Model;

/// <summary>
/// Thiết bị IoT vật lý (ESP32-C3 Water Sensor, hoặc các loại khác sau này).
/// Mỗi thiết bị có thể được gán vào 1 Batch để theo dõi cây trồng.
/// </summary>
public partial class IoTDevice
{
    public Guid Id { get; set; }

    /// <summary>Mã thiết bị (duy nhất). Ví dụ: "ESP001"</summary>
    public string DeviceCode { get; set; } = null!;

    /// <summary>Tên hiển thị. Ví dụ: "Cảm biến khu A1"</summary>
    public string DeviceName { get; set; } = null!;

    /// <summary>Địa chỉ MAC (tùy chọn, dùng để xác thực phần cứng)</summary>
    public string? MacAddress { get; set; }

    /// <summary>Loại thiết bị. Ví dụ: "ESP32C3_WaterSensor"</summary>
    public string DeviceType { get; set; } = "ESP32C3_WaterSensor";

    /// <summary>FK → Batch. Null nếu chưa gán.</summary>
    public Guid? BatchId { get; set; }

    /// <summary>Thiết bị có đang hoạt động không (do Manager bật/tắt thủ công)</summary>
    public bool IsActive { get; set; }

    /// <summary>Trạng thái (Inactive/Active). Tính theo BatchId + IsActive.</summary>
    public IoTDeviceStatus Status { get; set; } = IoTDeviceStatus.Inactive;

    public DateTime CreatedAt { get; set; }

    /// <summary>Thời gian nhận data MQTT cuối cùng</summary>
    public DateTime? LastActiveAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public virtual Batch? Batch { get; set; }

    public virtual ICollection<IoTDeviceSensor> DeviceSensors { get; set; } = new List<IoTDeviceSensor>();
}
