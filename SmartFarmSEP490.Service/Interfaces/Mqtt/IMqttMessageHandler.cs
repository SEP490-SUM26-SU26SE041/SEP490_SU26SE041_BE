using System;
using System.Threading.Tasks;

namespace SmartFarmSEP490.Service.Interfaces.Mqtt;

/// <summary>
/// Handler xử lý message nhận được từ MQTT broker.
/// </summary>
public interface IMqttMessageHandler
{
    /// <summary>
    /// Parse và xử lý 1 message MQTT.
    /// Topic format: "watersensor/{deviceCode}/data"
    /// Payload: JSON object chứa các trường cảm biến.
    /// </summary>
    Task HandleAsync(string topic, string payload);
}
