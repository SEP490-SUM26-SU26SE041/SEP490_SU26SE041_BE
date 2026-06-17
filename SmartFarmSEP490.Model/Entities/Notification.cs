using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartFarmSEP490.Model.Entities
{
    public class Notification
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid RecipientId { get; set; }

        public Guid? SenderId { get; set; }

        [Required]
        [MaxLength(50)]
        public string NotificationType { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        public string? Message { get; set; }

        [Required]
        public string Priority { get; set; } = "Low";

        [MaxLength(100)]
        public string? ReferenceTable { get; set; }

        public Guid? ReferenceId { get; set; }

        [NotMapped]
        public string? Metadata { get; set; } // Using NotMapped to avoid jsonb deserialization issues for now, or map it using Npgsql

        public bool IsRead { get; set; } = false;

        public DateTime? ReadAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("RecipientId")]
        public virtual User Recipient { get; set; } = null!;

        [ForeignKey("SenderId")]
        public virtual User? Sender { get; set; }
    }
}
