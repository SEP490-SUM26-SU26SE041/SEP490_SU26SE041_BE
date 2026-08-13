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
        // Cửa sổ "task hôm nay" theo ICT giờ làm việc: [00:00 → 17:00 ICT] = [00:00 → 10:00 UTC cùng ngày].
        // Reminder chạy lúc 16:00 ICT nên tất cả task due trong cửa sổ này (kể cả đã qua giờ nhắc) đều nhận được.
        var nowUtc = VietnamTime.NowUtc();
        var nowVietnam = VietnamTime.ToVietnam(nowUtc);
        var dayStartVietnam = nowVietnam.Date;
        var dayEndVietnam = dayStartVietnam.Date.AddHours(VietnamTime.DailyDeadlineHour); // 17:00 ICT hôm nay

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
