using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using SmartFarmSEP490.Model.DTOs;
using SmartFarmSEP490.Model.Entities;
using SmartFarmSEP490.Repository.Interfaces.Notifications;
using SmartFarmSEP490.Service.Hubs;
using SmartFarmSEP490.Service.Interfaces.Notifications;

namespace SmartFarmSEP490.Service.Services.Notifications
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationService(
            INotificationRepository notificationRepository, 
            IHubContext<NotificationHub> hubContext)
        {
            _notificationRepository = notificationRepository;
            _hubContext = hubContext;
        }

        public async Task<NotificationDto> PushNotificationAsync(CreateNotificationDto request)
        {
            var notification = new Notification
            {
                RecipientId = request.RecipientId,
                SenderId = request.SenderId,
                Title = request.Title,
                Message = request.Message,
                NotificationType = request.NotificationType,
                Priority = request.Priority,
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
                Priority = notification.Priority,
                IsRead = notification.IsRead,
                ReferenceTable = notification.ReferenceTable,
                ReferenceId = notification.ReferenceId,
                CreatedAt = notification.CreatedAt
            };

            // Push to SignalR clients connected as this user
            await _hubContext.Clients.Group($"User_{notification.RecipientId}").SendAsync("ReceiveNotification", dto);

            return dto;
        }

        public async Task<PaginatedList<NotificationDto>> GetUserNotificationsAsync(Guid userId, int pageNumber, int pageSize)
        {
            int skip = (pageNumber - 1) * pageSize;
            
            var notifications = await _notificationRepository.GetUserNotificationsAsync(userId, skip, pageSize);
            
            var dtoList = notifications.Select(n => new NotificationDto
            {
                Id = n.Id,
                RecipientId = n.RecipientId,
                SenderId = n.SenderId,
                Title = n.Title,
                Message = n.Message,
                NotificationType = n.NotificationType,
                Priority = n.Priority,
                IsRead = n.IsRead,
                ReadAt = n.ReadAt,
                ReferenceTable = n.ReferenceTable,
                ReferenceId = n.ReferenceId,
                CreatedAt = n.CreatedAt
            }).ToList();

            return new PaginatedList<NotificationDto>(dtoList, 1000, pageNumber, pageSize);
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
