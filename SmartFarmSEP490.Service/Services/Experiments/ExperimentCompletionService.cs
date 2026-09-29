using Microsoft.Extensions.Logging;
using SmartFarmSEP490.Model.Enums;
using SmartFarmSEP490.Repository.Interfaces.ExperimentBedAssignments;
using SmartFarmSEP490.Repository.Interfaces.Experiments;
using SmartFarmSEP490.Service.Interfaces.Experiments;

namespace SmartFarmSEP490.Service.Services.Experiments;

/// <summary>
/// Xử lý nghiệp vụ "hoàn thành" experiment:
///   - Auto-complete theo EndDate (gọi từ BackgroundService).
///   - Manual complete từ UpdateStatusAsync.
/// Cả hai đường đều release bed assignments.
/// </summary>
public class ExperimentCompletionService : IExperimentCompletionService
{
    private readonly IExperimentRepository _experimentRepository;
    private readonly IExperimentBedAssignmentRepository _bedAssignmentRepository;
    private readonly ILogger<ExperimentCompletionService> _logger;

    public ExperimentCompletionService(
        IExperimentRepository experimentRepository,
        IExperimentBedAssignmentRepository bedAssignmentRepository,
        ILogger<ExperimentCompletionService> logger)
    {
        _experimentRepository = experimentRepository;
        _bedAssignmentRepository = bedAssignmentRepository;
        _logger = logger;
    }

    public async Task<int> SweepExpiredAsync(CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var expired = await _experimentRepository.GetExpiredAsync(today);

        if (expired.Count == 0) return 0;

        _logger.LogInformation(
            "[ExperimentEndDateSweep] Found {Count} experiment(s) past EndDate (today UTC = {Today:O})",
            expired.Count, today);

        var succeeded = 0;
        foreach (var exp in expired)
        {
            ct.ThrowIfCancellationRequested();
            try
            {
                exp.Status = ExperimentStatus.Completed;
                exp.UpdatedAt = DateTime.UtcNow;
                await _bedAssignmentRepository.ReleaseBedsAsync(exp.Id);

                _logger.LogInformation(
                    "[ExperimentEndDateSweep] Auto-completed experiment {ExperimentId} (code={Code}, EndDate={EndDate:O})",
                    exp.Id, exp.ExperimentCode, exp.EndDate);
                succeeded++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "[ExperimentEndDateSweep] Failed to auto-complete experiment {ExperimentId} (code={Code})",
                    exp.Id, exp.ExperimentCode);
                // Tiếp tục với experiment khác, không break cả batch
            }
        }

        return succeeded;
    }

    public async Task CompleteAsync(Guid experimentId, CancellationToken ct = default)
    {
        var entity = await _experimentRepository.GetByIdAsync(experimentId);
        if (entity == null)
        {
            _logger.LogWarning(
                "[ExperimentCompletion] CompleteAsync: experiment {ExperimentId} not found",
                experimentId);
            return;
        }

        // Idempotent: nếu đã Completed rồi thì chỉ cần release (nếu chưa release)
        if (entity.Status != ExperimentStatus.Completed)
        {
            entity.Status = ExperimentStatus.Completed;
            entity.UpdatedAt = DateTime.UtcNow;
        }

        await _bedAssignmentRepository.ReleaseBedsAsync(experimentId);
    }
}
