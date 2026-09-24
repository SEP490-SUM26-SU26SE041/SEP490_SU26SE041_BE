using System;
using MQTTnet.Extensions.ManagedClient;

namespace SmartFarmSEP490.Service.Interfaces.Mqtt;

/// <summary>
/// Cấu hình MQTT từ appsettings.json (section "Mqtt").
/// Backend dùng TCP thuần (port 1883) - giống ESP32 firmware.
/// </summary>
public interface IMqttSettings
{
    bool Enabled { get; }
    string Host { get; }
    int Port { get; }
    string Username { get; }
    string Password { get; }
    string ClientId { get; }
    string SubscribeTopic { get; }
    int ReconnectDelaySeconds { get; }

    // Secondary broker (backup)
    string SecondaryHost { get; }
    int SecondaryPort { get; }
    string SecondaryClientId { get; }

    // Default DeviceCode - fallback khi topic không chứa DeviceCode hợp lệ
    string DefaultDeviceCode { get; }
}

public class MqttSettings : IMqttSettings
{
    public bool Enabled { get; set; } = true;
    public string Host { get; set; } = "broker.hivemq.com";
    public int Port { get; set; } = 1883;  // ✅ TCP thuần (giống ESP32)
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public string ClientId { get; set; } = $"smartfarm-backend-{Guid.NewGuid():N}";
    public string SubscribeTopic { get; set; } = "watersensor/+/data";
    public int ReconnectDelaySeconds { get; set; } = 10;

    // Secondary broker (backup)
    public string SecondaryHost { get; set; } = "broker.emqx.io";
    public int SecondaryPort { get; set; } = 1883;  // ✅ TCP thuần (giống ESP32)
    public string SecondaryClientId { get; set; } = $"smartfarm-backend-secondary-{Guid.NewGuid():N}";

    public string DefaultDeviceCode { get; set; } = "ESP001";

    /// <summary>
    /// Build MQTT options cho Primary broker - dùng TCP thuần.
    /// </summary>
    public ManagedMqttClientOptions BuildManagedClientOptions()
    {
        var builder = new MQTTnet.Client.MqttClientOptionsBuilder()
            .WithClientId(ClientId)
            .WithTcpServer(Host, Port)   // ✅ TCP thuần - không phải WebSocket
            .WithCleanSession()
            .WithKeepAlivePeriod(TimeSpan.FromSeconds(30))
            .WithTimeout(TimeSpan.FromSeconds(10));

        if (!string.IsNullOrEmpty(Username))
        {
            builder = builder.WithCredentials(Username, Password);
        }

        var clientOpts = builder.Build();

        return new ManagedMqttClientOptionsBuilder()
            .WithClientOptions(clientOpts)
            .WithAutoReconnectDelay(TimeSpan.FromSeconds(ReconnectDelaySeconds))
            .Build();
    }

    /// <summary>
    /// Build MQTT options cho Secondary broker - dùng TCP thuần.
    /// </summary>
    public ManagedMqttClientOptions BuildSecondaryManagedClientOptions()
    {
        var builder = new MQTTnet.Client.MqttClientOptionsBuilder()
            .WithClientId(SecondaryClientId)
            .WithTcpServer(SecondaryHost, SecondaryPort)   // ✅ TCP thuần
            .WithCleanSession()
            .WithKeepAlivePeriod(TimeSpan.FromSeconds(30))
            .WithTimeout(TimeSpan.FromSeconds(10));

        if (!string.IsNullOrEmpty(Username))
        {
            builder = builder.WithCredentials(Username, Password);
        }

        var clientOpts = builder.Build();

        return new ManagedMqttClientOptionsBuilder()
            .WithClientOptions(clientOpts)
            .WithAutoReconnectDelay(TimeSpan.FromSeconds(ReconnectDelaySeconds))
            .Build();
    }
}
