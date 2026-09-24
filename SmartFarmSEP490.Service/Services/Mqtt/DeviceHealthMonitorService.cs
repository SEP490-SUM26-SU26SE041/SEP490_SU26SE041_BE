using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SmartFarmSEP490.Model;
using SmartFarmSEP490.Model.Enums;
using SmartFarmSEP490.Repository.Interfaces.Alerts;
using SmartFarmSEP490.Repository.Interfaces.IoTDevices;

namespace SmartFarmSEP490.Service.Services.Mqtt;

/// <summary>
/// BackgroundService kiểm tra định kỳ các thiết bị IoT có đang offline không.
/// Khi phát hiện thiết bị offline quá ngưỡng → tạo Alert.
/// </summary>
public class DeviceHealthMonitorService : BackgroundService
{
    private readonly ILogger<DeviceHealthMonitorService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;

    public DeviceHealthMonitorService(
        ILogger<DeviceHealthMonitorService> logger,
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Đợi ứng dụng khởi động xong
        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);

        var intervalSeconds = GetIntervalSeconds();
        _logger.LogInformation(
            "[HealthMonitor] Started. Check interval = {Interval}s, Offline threshold = {Threshold}min",
            intervalSeconds, GetOfflineThresholdMinutes());

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckDevicesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[HealthMonitor] Error during health check");
            }

            await Task.Delay(TimeSpan.FromSeconds(intervalSeconds), stoppingToken);
        }
    }

    private async Task CheckDevicesAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var deviceRepo = scope.ServiceProvider.GetRequiredService<IIoTDeviceRepository>();
        var alertRepo = scope.ServiceProvider.GetRequiredService<IAlertRepository>();

        var thresholdMin = GetOfflineThresholdMinutes();
        var now = DateTime.UtcNow;

        // Lấy các thiết bị đang active và đã gán Batch
        var devices = await deviceRepo.GetActiveDevicesAsync();

        foreach (var device in devices)
        {
            if (ct.IsCancellationRequested) break;

            // Thiết bị chưa từng gửi data hoặc đã offline
            if (!device.LastActiveAt.HasValue)
                continue;

            var minutesOffline = (now - device.LastActiveAt.Value).TotalMinutes;
            if (minutesOffline <= thresholdMin)
                continue;

            // Tránh spam alert: kiểm tra xem đã có alert unresolved cho device này chưa
            var existingAlerts = await alertRepo.GetByExperimentAsync(device.Batch!.ExperimentId);
            var alreadyAlerted = existingAlerts.Any(a =>
                a.IsResolved == false &&
                a.Title != null &&
                a.Title.Contains(device.DeviceCode));

            if (alreadyAlerted)
                continue;

            var alert = new Alert
            {
                Id = Guid.NewGuid(),
                ExperimentId = device.Batch?.ExperimentId,
                BatchId = device.BatchId,
                SensorId = null,
                Title = $"Thiết bị {device.DeviceCode} offline",
                Message = $"Thiết bị IoT '{device.DeviceCode}' đã không gửi dữ liệu trong {Math.Round(minutesOffline)} phút. Vui lòng kiểm tra kết nối.",
                Severity = AlertSeverity.High,
                IsResolved = false,
                CreatedAt = now
            };

            await alertRepo.AddAsync(alert);
            await deviceRepo.SaveChangesAsync();

            _logger.LogWarning(
                "[HealthMonitor] ⚠️ Device {DeviceCode} offline {Minutes:F1} min. Alert created.",
                device.DeviceCode, minutesOffline);
        }
    }

    private int GetIntervalSeconds() =>
        int.TryParse(_configuration["IoT:CheckIntervalSeconds"], out var v) ? v : 60;

    private int GetOfflineThresholdMinutes() =>
        int.TryParse(_configuration["IoT:OfflineThresholdMinutes"], out var v) ? v : 5;
}
