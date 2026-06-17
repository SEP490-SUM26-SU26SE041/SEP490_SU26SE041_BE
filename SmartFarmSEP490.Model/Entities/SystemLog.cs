using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartFarmSEP490.Model
{
    public class SystemLog
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid? UserId { get; set; }
        
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [Required]
        [MaxLength(100)]
        public string Action { get; set; }

        [MaxLength(100)]
        public string? EntityName { get; set; }

        public Guid? EntityId { get; set; }

        public string? Description { get; set; }

        [MaxLength(45)]
        public string? IpAddress { get; set; }

        public string? UserAgent { get; set; }

        [Column(TypeName = "jsonb")]
        public System.Text.Json.JsonDocument? Metadata { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
