using Microsoft.Extensions.Logging;
using SmartFarmSEP490.Model.DTOs;
using SmartFarmSEP490.Repository.Interfaces.Tasks;
using SmartFarmSEP490.Service.Interfaces.Notifications;
using SmartFarmSEP490.Service.Interfaces.Tasks;

namespace SmartFarmSEP490.Service.Services.Tasks;

public class OverdueTaskService : IOverdueTaskService
{
    private readonly ITaskRepository _taskRepository;
    private readonly INotificationService _notificationService;
    private readonly ILogger<OverdueTaskService> _logger;

    public OverdueTaskService(
        ITaskRepository taskRepository,
        INotificationService notificationService,
        ILogger<OverdueTaskService> logger)
    {
        _taskRepository = taskRepository;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<int> SweepAsync(CancellationToken ct = default)
    {
        var nowUtc = DateTime.UtcNow;

        // Snapshot trước khi UPDATE để biết những task nào sắp bị đánh overdue
        var candidates = await _taskRepository.GetOverdueCandidatesAsync(nowUtc, ct);

        if (candidates.Count == 0)
            return 0;

        var affected = await _taskRepository.MarkOverdueAsync(nowUtc, ct);

        if (affected > 0)
        {
            _logger.LogInformation(
                "[OverdueSweep] {Count} task(s) marked Overdue at {Now:O} UTC",
                affected, nowUtc);
        }

        // Push notification cho mỗi task thực sự bị ảnh hưởng
        // (candidates có thể rộng hơn affected nếu chạy 2 sweep liên tiếp — chỉ push cho những task được cập nhật)
        foreach (var task in candidates)
        {
            try
            {
                // 1) Notify assignee (nếu có)
                if (task.AssignedTo.HasValue && task.AssignedTo.Value != Guid.Empty)
                {
                    await _notificationService.PushNotificationAsync(new CreateNotificationDto
                    {
                        RecipientId = task.AssignedTo.Value,
                        NotificationType = "TaskOverdue",
                        Title = "Task đã quá hạn",
                        Message = $"Task \"{task.Title}\" đã quá hạn. Vui lòng xử lý hoặc báo cáo.",
                        Priority = "High",
                        ReferenceTable = "Task",
                        ReferenceId = task.Id
                    });
                }

                // 2) Notify researcher (task creator)
                if (task.CreatedBy.HasValue && task.CreatedBy.Value != Guid.Empty
                    && task.CreatedBy.Value != task.AssignedTo)
                {
                    await _notificationService.PushNotificationAsync(new CreateNotificationDto
                    {
                        RecipientId = task.CreatedBy.Value,
                        NotificationType = "TaskOverdue",
                        Title = "Task bạn tạo đã quá hạn",
                        Message = $"Task \"{task.Title}\" đã quá hạn và chưa hoàn thành.",
                        Priority = "Medium",
                        ReferenceTable = "Task",
                        ReferenceId = task.Id
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[OverdueSweep] Failed to push notification for task {TaskId}", task.Id);
            }
        }

        return affected;
    }
}