using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SmartFarmSEP490.Model.Helpers;
using SmartFarmSEP490.Service.Interfaces.Experiments;

namespace SmartFarmSEP490.Service.Services.Experiments;

/// <summary>
/// BackgroundService quét experiment quá EndDate mỗi ngày đúng giờ cố định theo giờ Việt Nam (ICT = UTC+7).
/// - Mặc định sweep lúc 00:30 ICT (sau OverdueTaskSweep lúc 17:01 ICT ngày hôm trước, tránh spike).
/// - Với mỗi experiment EndDate &lt; today và đang Active/Paused: chuyển Completed + release beds.
/// - Idempotent — chạy nhiều lần vẫn an toàn (Completed là terminal).
/// </summary>
public class ExperimentEndDateSweepBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ExperimentEndDateSweepBackgroundService> _logger;

    /// <summary>Giờ chạy sweep theo giờ Việt Nam. Mặc định 00:30 ICT.</summary>
    public static readonly TimeSpan SweepTimeOfDayVietnam = new(0, 30, 0);

    public ExperimentEndDateSweepBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<ExperimentEndDateSweepBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "[ExperimentEndDateSweep] Started. Sweep time = {Time:hh\\:mm} ICT (UTC+7)",
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
                    "[ExperimentEndDateSweep] Next sweep in {Delay:hh\\:mm\\:ss}",
                    delay);

                try { await Task.Delay(delay, stoppingToken); }
                catch (OperationCanceledException) { break; }

                await RunSweepAsync(stoppingToken);
            }
            catch (OperationCanceledException) { break; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ExperimentEndDateSweep] Loop error; sleeping 60s before retry");
                try { await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken); }
                catch (OperationCanceledException) { break; }
            }
        }

        _logger.LogInformation("[ExperimentEndDateSweep] Stopped");
    }

    /// <summary>
    /// Tính delay tới lần sweep kế tiếp, theo UTC.
    /// Sweep hour là <see cref="SweepTimeOfDayVietnam"/> theo ICT = UTC+7.
    /// </summary>
    private static TimeSpan ComputeDelayUntilNextSweepUtc()
    {
        var nowUtc = DateTime.UtcNow;
        var nowVietnam = VietnamTime.ToVietnam(nowUtc);

        var nextSweepVietnam = nowVietnam.Date.Add(SweepTimeOfDayVietnam);
        if (nowVietnam >= nextSweepVietnam)
            nextSweepVietnam = nextSweepVietnam.AddDays(1);

        // ICT - 7h = UTC
        var nextSweepUtc = nextSweepVietnam.AddHours(-7);
        return nextSweepUtc - nowUtc;
    }

    private async Task RunSweepAsync(CancellationToken ct)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var completionSvc = scope.ServiceProvider.GetRequiredService<IExperimentCompletionService>();
            var affected = await completionSvc.SweepExpiredAsync(ct);

            if (affected > 0)
            {
                var nowUtc = DateTime.UtcNow;
                _logger.LogInformation(
                    "[ExperimentEndDateSweep] {Count} experiment(s) auto-completed at {Now:O} UTC ({Vietnam:o} ICT)",
                    affected, nowUtc, VietnamTime.ToVietnam(nowUtc));
            }
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ExperimentEndDateSweep] Sweep tick failed");
        }
    }
}
