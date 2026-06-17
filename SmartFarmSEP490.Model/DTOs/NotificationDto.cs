using System;

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
        public string Priority { get; set; } = "Low";
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
        public string Priority { get; set; } = "Low";
        public string? ReferenceTable { get; set; }
        public Guid? ReferenceId { get; set; }
    }
}
