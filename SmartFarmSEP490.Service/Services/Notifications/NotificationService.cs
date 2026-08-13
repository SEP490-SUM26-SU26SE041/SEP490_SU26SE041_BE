using System;
using System.Linq;
using System.Threading.Tasks;
using SmartFarmSEP490.Model.DTOs;
using SmartFarmSEP490.Model.Entities;
using SmartFarmSEP490.Model.Enums;
using SmartFarmSEP490.Repository.Interfaces.Notifications;
using SmartFarmSEP490.Service.Helpers;
using SmartFarmSEP490.Service.Interfaces.Notifications;
using SmartFarmSEP490.Service.WebSockets;

namespace SmartFarmSEP490.Service.Services.Notifications
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IWebSocketConnectionManager _wsManager;

        public NotificationService(
            INotificationRepository notificationRepository,
            IWebSocketConnectionManager wsManager)
        {
            _notificationRepository = notificationRepository;
            _wsManager = wsManager;
        }

        public async Task<NotificationDto> PushNotificationAsync(CreateNotificationDto request)
        {
            // Parse priority string to enum (handles null/empty gracefully)
            if (!Enum.TryParse<AlertSeverity>(request.Priority, true, out var priority))
                priority = AlertSeverity.Medium;

            var notification = new Notification
            {
                RecipientId = request.RecipientId,
                SenderId = request.SenderId,
                Title = request.Title,
                Message = request.Message,
                NotificationType = request.NotificationType,
                Priority = priority,
                ReferenceTable = request.ReferenceTable,
                ReferenceId = request.ReferenceId
            };

            await _notificationRepository.AddNotificationAsync(notification);

            var dto = new NotificationDto
            {
                Id = notification.Id,
                RecipientId = notification.RecipientId,
                SenderId = notification.SenderId,
                Title = notification.Title,
                Message = notification.Message,
                NotificationType = notification.NotificationType,
                Priority = notification.Priority.ToString(),
                IsRead = notification.IsRead,
                ReferenceTable = notification.ReferenceTable,
                ReferenceId = notification.ReferenceId,
                CreatedAt = notification.CreatedAt,
                CreatedAtVietnam = VietnamTime.ToVietnamOffset(notification.CreatedAt)
            };

            // Push realtime tới mọi WebSocket connection đang mở của user
            try
            {
                await _wsManager.SendToUserAsync(notification.RecipientId, "ReceiveNotification", dto);
            }
            catch (Exception)
            {
                // Realtime failure không nên nuốt notification đã lưu DB
            }

            return dto;
        }

        public async Task<PaginatedList<NotificationDto>> GetUserNotificationsAsync(Guid userId, int pageNumber, int pageSize)
        {
            int skip = (pageNumber - 1) * pageSize;

            var notifications = await _notificationRepository.GetUserNotificationsAsync(userId, skip, pageSize);
            var totalCount = await _notificationRepository.GetTotalCountAsync(userId);

            var dtoList = notifications.Select(n => new NotificationDto
            {
                Id = n.Id,
                RecipientId = n.RecipientId,
                SenderId = n.SenderId,
                Title = n.Title,
                Message = n.Message,
                NotificationType = n.NotificationType,
                Priority = n.Priority.ToString(),
                IsRead = n.IsRead,
                ReadAt = n.ReadAt,
                ReferenceTable = n.ReferenceTable,
                ReferenceId = n.ReferenceId,
                CreatedAt = n.CreatedAt,
                CreatedAtVietnam = VietnamTime.ToVietnamOffset(n.CreatedAt)
            }).ToList();

            return new PaginatedList<NotificationDto>(dtoList, totalCount, pageNumber, pageSize);
        }

        public async Task<int> GetUnreadCountAsync(Guid userId)
        {
            return await _notificationRepository.GetUnreadCountAsync(userId);
        }

        public async Task<bool> MarkAsReadAsync(Guid notificationId, Guid userId)
        {
            var notification = await _notificationRepository.GetNotificationByIdAsync(notificationId);
            if (notification == null || notification.RecipientId != userId)
                return false;

            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            await _notificationRepository.UpdateNotificationAsync(notification);
            return true;
        }

        public async Task MarkAllAsReadAsync(Guid userId)
        {
            await _notificationRepository.MarkAllAsReadAsync(userId);
        }
    }
}
