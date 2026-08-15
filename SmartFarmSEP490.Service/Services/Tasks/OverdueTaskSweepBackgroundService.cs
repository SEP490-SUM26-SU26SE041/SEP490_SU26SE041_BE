using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SmartFarmSEP490.Model.Helpers;
using SmartFarmSEP490.Service.Interfaces.Tasks;

namespace SmartFarmSEP490.Service.Services.Tasks;

/// <summary>
/// BackgroundService quét task quá hạn mỗi ngày đúng giờ cố định theo giờ Việt Nam (ICT = UTC+7).
/// - Mặc định sweep lúc 00:00 ICT.
/// - Đổi giờ bằng cách sửa <see cref="SweepTimeOfDayVietnam"/> trước khi build (const) hoặc tách config.
/// - Toàn bộ DateTime so sánh theo UTC; FE hiển thị convert từ UTC sang giờ VN.
/// </summary>
public class OverdueTaskSweepBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OverdueTaskSweepBackgroundService> _logger;

    /// <summary>Giờ chạy sweep theo giờ Việt Nam. Mặc định 17:01 ICT — 1 phút sau deadline 17:00 ICT (10:00 UTC).</summary>
    public static readonly TimeSpan SweepTimeOfDayVietnam = new(17, 1, 0);

    public OverdueTaskSweepBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<OverdueTaskSweepBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "[OverdueSweep] Started. Sweep time = {Time:hh\\:mm} ICT (UTC+7, 1 min after 17:00 deadline)",
            SweepTimeOfDayVietnam);

        // Stagger 30s để tránh spike lúc app start
        try { await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken); }
        catch (OperationCanceledException) { return; }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var delay = ComputeDelayUntilNextSweepUtc();
                _logger.LogInformation(
                    "[OverdueSweep] Next sweep in {Delay:hh\\:mm\\:ss} (UTC trigger time below)",
                    delay);

                try { await Task.Delay(delay, stoppingToken); }
                catch (OperationCanceledException) { break; }

                await RunSweepAsync(stoppingToken);
            }
            catch (OperationCanceledException) { break; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[OverdueSweep] Loop error; sleeping 60s before retry");
                try { await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken); }
                catch (OperationCanceledException) { break; }
            }
        }

        _logger.LogInformation("[OverdueSweep] Stopped");
    }

    /// <summary>
    /// Tính delay tới lần sweep kế tiếp, tính theo UTC.
    /// Sweep hour là <see cref="SweepTimeOfDayVietnam"/> theo ICT = UTC+7, cộng thêm vào sau khi so với giờ VN hiện tại.
    /// </summary>
    private static TimeSpan ComputeDelayUntilNextSweepUtc()
    {
        var nowUtc = DateTime.UtcNow;
        var nowVietnam = VietnamTime.ToVietnam(nowUtc);

        var nextSweepVietnam = nowVietnam.Date.Add(SweepTimeOfDayVietnam);
        if (nowVietnam >= nextSweepVietnam)
            nextSweepVietnam = nextSweepVietnam.AddDays(1);

        // Convert sang UTC: ICT - 7h = UTC
        var nextSweepUtc = nextSweepVietnam.AddHours(-7);
        return nextSweepUtc - nowUtc;
    }

    private async Task RunSweepAsync(CancellationToken ct)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var overdueSvc = scope.ServiceProvider.GetRequiredService<IOverdueTaskService>();
            var affected = await overdueSvc.SweepAsync(ct);

            if (affected > 0)
            {
                var nowUtc = DateTime.UtcNow;
                _logger.LogInformation(
                    "[OverdueSweep] {Count} task(s) marked Overdue at {Now:O} UTC ({Vietnam:o} ICT)",
                    affected, nowUtc, VietnamTime.ToVietnam(nowUtc));
            }
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[OverdueSweep] Sweep tick failed");
        }
    }
}
