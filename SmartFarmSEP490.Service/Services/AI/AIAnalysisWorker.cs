using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SmartFarmSEP490.Service.Interfaces.AI;
using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace SmartFarmSEP490.Service.Services.AI;

/// <summary>
/// BackgroundService consume từ Channel&lt;Guid&gt; và gọi AIAnalysisService.ProcessAsync.
/// </summary>
public class AIAnalysisWorker : BackgroundService
{
    private readonly Channel<Guid> _queue;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AIAnalysisWorker> _logger;

    public AIAnalysisWorker(
        Channel<Guid> queue,
        IServiceProvider serviceProvider,
        ILogger<AIAnalysisWorker> logger)
    {
        _queue = queue;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("[AIWorker] Started, listening on queue.");

        await foreach (var plantImageId in _queue.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var service = scope.ServiceProvider.GetRequiredService<IAIAnalysisService>();
                await service.ProcessAsync(plantImageId, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "[AIWorker] Unhandled exception processing PlantImage {Id}. Continuing.",
                    plantImageId);
                // Không crash loop — log và tiếp tục job kế tiếp
            }
        }

        _logger.LogInformation("[AIWorker] Stopped.");
    }
}
