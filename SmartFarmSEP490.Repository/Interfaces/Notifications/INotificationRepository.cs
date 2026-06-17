using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SmartFarmSEP490.Model.Entities;

namespace SmartFarmSEP490.Repository.Interfaces.Notifications
{
    public interface INotificationRepository
    {
        Task<Notification> AddNotificationAsync(Notification notification);
        Task<List<Notification>> GetUserNotificationsAsync(Guid userId, int skip, int take);
        Task<int> GetUnreadCountAsync(Guid userId);
        Task<Notification?> GetNotificationByIdAsync(Guid id);
        Task UpdateNotificationAsync(Notification notification);
        Task MarkAllAsReadAsync(Guid userId);
    }
}
