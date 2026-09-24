using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;
using SmartFarmSEP490.Service.Interfaces.Mqtt;

namespace SmartFarmSEP490.Service.Services.Mqtt;

/// <summary>
/// BackgroundService subscribe MQTT broker để nhận dữ liệu từ ESP32-C3.
/// Dùng TCP thuần (port 1883) - giống ESP32 firmware để data chắc chắn về DB.
/// </summary>
public class MqttHostedService : BackgroundService
{
    private readonly ILogger<MqttHostedService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly MqttSettings _settings;

    private IManagedMqttClient? _primaryClient;
    private IManagedMqttClient? _secondaryClient;

    public MqttHostedService(
        ILogger<MqttHostedService> logger,
        IServiceScopeFactory scopeFactory,
        IOptions<MqttSettings> settings)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
        _settings = settings.Value;
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("[MQTT] Service starting...");

        if (!_settings.Enabled)
        {
            _logger.LogWarning("[MQTT] Service is DISABLED in configuration. Skipping startup.");
            await base.StartAsync(cancellationToken);
            return;
        }

        var factory = new MqttFactory();

        // Tạo Primary client
        _primaryClient = factory.CreateManagedMqttClient();
        _primaryClient.ConnectingFailedAsync += args =>
        {
            _logger.LogWarning("[MQTT-Primary] Connection failed: {Reason}",
                args.Exception?.Message ?? "unknown");
            return Task.CompletedTask;
        };
        _primaryClient.ConnectionStateChangedAsync += args =>
        {
            _logger.LogInformation("[MQTT-Primary] Connection state changed");
            return Task.CompletedTask;
        };
        _primaryClient.DisconnectedAsync += args =>
        {
            _logger.LogWarning("[MQTT-Primary] Disconnected: {Reason}", args.Reason);
            return Task.CompletedTask;
        };
        _primaryClient.ApplicationMessageReceivedAsync += OnMessageReceivedAsync;

        // Tạo Secondary client
        _secondaryClient = factory.CreateManagedMqttClient();
        _secondaryClient.ConnectingFailedAsync += args =>
        {
            _logger.LogWarning("[MQTT-Secondary] Connection failed: {Reason}",
                args.Exception?.Message ?? "unknown");
            return Task.CompletedTask;
        };
        _secondaryClient.ConnectionStateChangedAsync += args =>
        {
            _logger.LogInformation("[MQTT-Secondary] Connection state changed");
            return Task.CompletedTask;
        };
        _secondaryClient.DisconnectedAsync += args =>
        {
            _logger.LogWarning("[MQTT-Secondary] Disconnected: {Reason}", args.Reason);
            return Task.CompletedTask;
        };
        _secondaryClient.ApplicationMessageReceivedAsync += OnMessageReceivedAsync;

        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_settings.Enabled) return;

        // Chờ ứng dụng khởi động xong
        await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);

        // ✅ Kết nối song song 2 broker
        var primaryTask = ConnectWithRetryAsync(
            _primaryClient!,
            () => _primaryClient!.StartAsync(_settings.BuildManagedClientOptions()),
            "Primary",
            stoppingToken);

        var secondaryTask = ConnectWithRetryAsync(
            _secondaryClient!,
            () => _secondaryClient!.StartAsync(_settings.BuildSecondaryManagedClientOptions()),
            "Secondary",
            stoppingToken);

        await Task.WhenAll(primaryTask, secondaryTask);
    }

    private async Task ConnectWithRetryAsync(
        IManagedMqttClient client,
        Func<Task> startAction,
        string label,
        CancellationToken stoppingToken)
    {
        if (client == null)
        {
            _logger.LogError("[MQTT-{Label}] Client is null. Cannot connect.", label);
            return;
        }

        var attempt = 0;
        var maxDelay = TimeSpan.FromSeconds(60);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                attempt++;
                var host = label == "Primary" ? _settings.Host : _settings.SecondaryHost;
                var port = label == "Primary" ? _settings.Port : _settings.SecondaryPort;

                _logger.LogInformation("[MQTT-{Label}] Connecting to tcp://{Host}:{Port} (attempt {Attempt})",
                    label, host, port, attempt);

                await startAction();

                // Subscribe sau khi connect thành công
                var factory = new MqttFactory();
                var topicFilter = factory.CreateTopicFilterBuilder()
                    .WithTopic(_settings.SubscribeTopic)
                    .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                    .Build();

                await client.SubscribeAsync(new[] { topicFilter });
                _logger.LogInformation("[MQTT-{Label}] ✅ Connected & subscribed to '{Topic}'",
                    label, _settings.SubscribeTopic);

                // Reset retry counter khi connect thành công
                attempt = 0;

                // Giữ connection sống
                while (!stoppingToken.IsCancellationRequested && client.IsConnected)
                {
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }

                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[MQTT-{Label}] ❌ Connect failed (attempt {Attempt}): {Message}",
                    label, attempt, ex.Message);
                var delay = TimeSpan.FromSeconds(Math.Min(Math.Pow(2, attempt), maxDelay.TotalSeconds));
                _logger.LogInformation("[MQTT-{Label}] Retrying in {Seconds}s...", label, delay.TotalSeconds);
                try
                {
                    await Task.Delay(delay, stoppingToken);
                }
                catch (TaskCanceledException) { return; }
            }
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("[MQTT] Service stopping...");
        if (_primaryClient != null)
        {
            await _primaryClient.StopAsync(true);
            _primaryClient.Dispose();
        }
        if (_secondaryClient != null)
        {
            await _secondaryClient.StopAsync(true);
            _secondaryClient.Dispose();
        }
        await base.StopAsync(cancellationToken);
    }

    /// <summary>
    /// Callback khi nhận message từ MQTT broker.
    /// </summary>
    private async Task OnMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs e)
    {
        var topic = e.ApplicationMessage.Topic;
        var payload = e.ApplicationMessage.PayloadSegment.Count > 0
            ? System.Text.Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment.ToArray())
            : string.Empty;

        // ✅ LOG RÕ RÀNG để debug
        _logger.LogInformation("[MQTT] 📥 Received | Topic: {Topic} | Payload: {Payload}",
            topic, payload);

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var handler = scope.ServiceProvider.GetRequiredService<IMqttMessageHandler>();
            await handler.HandleAsync(topic, payload);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[MQTT] Error handling message from topic {Topic}", topic);
        }
    }
}
