using Microsoft.Extensions.Logging;
using SmartFarmSEP490.Repository.Interfaces.Tasks;
using SmartFarmSEP490.Service.Interfaces.Tasks;

namespace SmartFarmSEP490.Service.Services.Tasks;

public class OverdueTaskService : IOverdueTaskService
{
    private readonly ITaskRepository _taskRepository;
    private readonly ILogger<OverdueTaskService> _logger;

    public OverdueTaskService(ITaskRepository taskRepository, ILogger<OverdueTaskService> logger)
    {
        _taskRepository = taskRepository;
        _logger = logger;
    }

    public async Task<int> SweepAsync(CancellationToken ct = default)
    {
        var nowUtc = DateTime.UtcNow;
        var affected = await _taskRepository.MarkOverdueAsync(nowUtc, ct);

        if (affected > 0)
        {
            _logger.LogInformation(
                "[OverdueSweep] {Count} task(s) marked Overdue at {Now:O} UTC (lazy path)",
                affected, nowUtc);
        }

        return affected;
    }
}