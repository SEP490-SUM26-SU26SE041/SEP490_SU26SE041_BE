using System;
using SmartFarmSEP490.Model.Enums;

namespace SmartFarmSEP490.Model.DTOs
{
    public class NotificationDto
    {
        public Guid Id { get; set; }
        public Guid RecipientId { get; set; }
        public Guid? SenderId { get; set; }
        public string NotificationType { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Message { get; set; }
        public string Priority { get; set; } = "Medium";  // serialized as string for FE compatibility
        public string? ReferenceTable { get; set; }
        public Guid? ReferenceId { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateNotificationDto
    {
        public Guid RecipientId { get; set; }
        public Guid? SenderId { get; set; }
        public string NotificationType { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Message { get; set; }
        public string Priority { get; set; } = "Medium";  // "Low", "Medium", "High", "Critical" - serialized as string
        public string? ReferenceTable { get; set; }
        public Guid? ReferenceId { get; set; }
    }
}
