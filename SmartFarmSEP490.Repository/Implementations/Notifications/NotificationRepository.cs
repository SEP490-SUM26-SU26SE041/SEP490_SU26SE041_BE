using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmartFarmSEP490.Model.Entities;
using SmartFarmSEP490.Repository.DbContexts;
using SmartFarmSEP490.Repository.Interfaces.Notifications;

namespace SmartFarmSEP490.Repository.Implementations.Notifications
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly SmartFarmDbContext _context;

        public NotificationRepository(SmartFarmDbContext context)
        {
            _context = context;
        }

        public async Task<Notification> AddNotificationAsync(Notification notification)
        {
            await _context.Notifications.AddAsync(notification);
            await _context.SaveChangesAsync();
            return notification;
        }

        public async Task<List<Notification>> GetUserNotificationsAsync(Guid userId, int skip, int take)
        {
            return await _context.Notifications
                .Where(n => n.RecipientId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        public async Task<int> GetTotalCountAsync(Guid userId)
        {
            return await _context.Notifications.CountAsync(n => n.RecipientId == userId);
        }

        public async Task<int> GetUnreadCountAsync(Guid userId)
        {
            return await _context.Notifications
                .CountAsync(n => n.RecipientId == userId && !n.IsRead);
        }

        public async Task<Notification?> GetNotificationByIdAsync(Guid id)
        {
            return await _context.Notifications.FindAsync(id);
        }

        public async Task UpdateNotificationAsync(Notification notification)
        {
            _context.Notifications.Update(notification);
            await _context.SaveChangesAsync();
        }

        public async Task MarkAllAsReadAsync(Guid userId)
        {
            var unreadLogs = await _context.Notifications
                .Where(n => n.RecipientId == userId && !n.IsRead)
                .ToListAsync();

            foreach (var log in unreadLogs)
            {
                log.IsRead = true;
                log.ReadAt = DateTime.UtcNow;
            }

            if (unreadLogs.Any())
            {
                await _context.SaveChangesAsync();
            }
        }
    }
}
