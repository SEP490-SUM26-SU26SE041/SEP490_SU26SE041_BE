using System;
using System.Collections.Generic;

namespace SmartFarmSEP490.Model;

/// <summary>
/// Bảng mapping giữa IoTDevice và Sensor.
/// Mỗi bản ghi đại diện cho một cặp (Device, Sensor) và field MQTT tương ứng.
/// Ví dụ: Device ESP001 + Sensor "pH-001" → field MQTT "ph"
/// </summary>
public partial class IoTDeviceSensor
{
    public Guid Id { get; set; }

    /// <summary>FK → IoTDevice</summary>
    public Guid IoTDeviceId { get; set; }

    /// <summary>FK → Sensor</summary>
    public Guid SensorId { get; set; }

    /// <summary>Tên field trong JSON MQTT. Ví dụ: "dht_h", "ph", "ds18b20_t"</summary>
    public string MqttFieldName { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public virtual IoTDevice IoTDevice { get; set; } = null!;

    public virtual Sensor Sensor { get; set; } = null!;
}
