using SmartFarmSEP490.Repository.Interfaces.Tasks;
using SmartFarmSEP490.Service.Helpers;
using SmartFarmSEP490.Service.Interfaces.Notifications;

namespace SmartFarmSEP490.Service.Services.Tasks;

public interface IReminderTaskService
{
    /// <summary>
    /// Gửi notification nhắc nhở cho các task chưa hoàn thành có DueDate trong ngày hiện tại.
    /// </summary>
    /// <returns>Số notification đã gửi.</returns>
    Task<int> SendDailyReminderAsync(CancellationToken ct = default);
}

public class ReminderTaskService : IReminderTaskService
{
    private readonly ITaskRepository _taskRepository;
    private readonly INotificationService _notificationService;

    public ReminderTaskService(
        ITaskRepository taskRepository,
        INotificationService notificationService)
    {
        _taskRepository = taskRepository;
        _notificationService = notificationService;
    }

    public async Task<int> SendDailyReminderAsync(CancellationToken ct = default)
    {
        // Lấy cửa sổ ngày theo ICT
        var nowVietnam = VietnamTime.ToVietnam(DateTime.UtcNow);
        var dayStartVietnam = nowVietnam.Date;
        var dayEndVietnam = dayStartVietnam.AddDays(1);

        var dayStartUtc = VietnamTime.ToUtcFromVietnam(dayStartVietnam);
        var dayEndUtc = VietnamTime.ToUtcFromVietnam(dayEndVietnam);

        var tasks = await _taskRepository.GetActiveTasksDueOnDayAsync(dayStartUtc, dayEndUtc, ct);
        if (tasks.Count == 0) return 0;

        var sent = 0;
        foreach (var task in tasks)
        {
            if (!task.AssignedTo.HasValue || task.AssignedTo.Value == Guid.Empty)
                continue;

            var dueVietnam = task.DueDate.HasValue
                ? VietnamTime.ToVietnam(task.DueDate.Value)
                : (DateTime?)null;
            var dueText = dueVietnam.HasValue
                ? dueVietnam.Value.ToString("HH:mm 'ICT' dd/MM")
                : "hôm nay";

            try
            {
                await _notificationService.PushNotificationAsync(new Model.DTOs.CreateNotificationDto
                {
                    RecipientId = task.AssignedTo.Value,
                    NotificationType = "TaskReminder",
                    Title = "Nhắc nhở: Task cần hoàn thành hôm nay",
                    Message = $"Task \"{task.Title}\" cần hoàn thành trước {dueText}.",
                    Priority = "High",
                    ReferenceTable = "Task",
                    ReferenceId = task.Id
                });
                sent++;
            }
            catch { /* nuốt lỗi từng task để không chặn các task sau */ }
        }
        return sent;
    }
}
