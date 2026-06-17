using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SmartFarmSEP490.Model.DTOs;

namespace SmartFarmSEP490.Service.Interfaces.Notifications
{
    public interface INotificationService
    {
        Task<NotificationDto> PushNotificationAsync(CreateNotificationDto request);
        Task<PaginatedList<NotificationDto>> GetUserNotificationsAsync(Guid userId, int pageNumber, int pageSize);
        Task<int> GetUnreadCountAsync(Guid userId);
        Task<bool> MarkAsReadAsync(Guid notificationId, Guid userId);
        Task MarkAllAsReadAsync(Guid userId);
    }
}
