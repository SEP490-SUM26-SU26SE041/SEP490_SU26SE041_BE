using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SmartFarmSEP490.Service.Helpers;

namespace SmartFarmSEP490.Service.Services.Tasks;

/// <summary>
/// BackgroundService nhắc nhở các task chưa hoàn thành có DueDate trong ngày.
/// Mặc định chạy lúc 16:30 ICT (UTC+7) hằng ngày — trước thời điểm overdue sweep 17:00.
/// </summary>
public class ReminderSweepBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ReminderSweepBackgroundService> _logger;

    /// <summary>Giờ chạy reminder theo giờ Việt Nam (UTC+7). Mặc định 16:30.</summary>
    public static readonly TimeSpan ReminderTimeOfDayVietnam = new(16, 30, 0);

    public ReminderSweepBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<ReminderSweepBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "[ReminderSweep] Started. Reminder time = {Time:hh\\:mm} ICT (UTC+7)",
            ReminderTimeOfDayVietnam);

        // Stagger 30s sau khi start
        try { await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken); }
        catch (OperationCanceledException) { return; }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var delay = ComputeDelayUntilNextReminderUtc();
                _logger.LogInformation(
                    "[ReminderSweep] Next reminder in {Delay:hh\\:mm\\:ss}",
                    delay);

                try { await Task.Delay(delay, stoppingToken); }
                catch (OperationCanceledException) { break; }

                await RunReminderAsync(stoppingToken);
            }
            catch (OperationCanceledException) { break; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ReminderSweep] Loop error; sleeping 60s before retry");
                try { await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken); }
                catch (OperationCanceledException) { break; }
            }
        }

        _logger.LogInformation("[ReminderSweep] Stopped");
    }

    private static TimeSpan ComputeDelayUntilNextReminderUtc()
    {
        var nowUtc = DateTime.UtcNow;
        var nowVietnam = VietnamTime.ToVietnam(nowUtc);

        var nextReminderVietnam = nowVietnam.Date.Add(ReminderTimeOfDayVietnam);
        if (nowVietnam >= nextReminderVietnam)
            nextReminderVietnam = nextReminderVietnam.AddDays(1);

        // ICT - 7h = UTC
        var nextReminderUtc = nextReminderVietnam.AddHours(-7);
        return nextReminderUtc - nowUtc;
    }

    private async Task RunReminderAsync(CancellationToken ct)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var reminderSvc = scope.ServiceProvider.GetRequiredService<IReminderTaskService>();
            var sent = await reminderSvc.SendDailyReminderAsync(ct);

            if (sent > 0)
            {
                var nowUtc = DateTime.UtcNow;
                _logger.LogInformation(
                    "[ReminderSweep] {Count} reminder(s) sent at {Now:O} UTC ({Vietnam:o} ICT)",
                    sent, nowUtc, VietnamTime.ToVietnam(nowUtc));
            }
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ReminderSweep] Tick failed");
        }
    }
}
