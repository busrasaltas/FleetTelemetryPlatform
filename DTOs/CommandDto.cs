using System.ComponentModel.DataAnnotations;

namespace FleetTelemetryPlatform.DTOs
{
    public class IssueCommandDto
    {
        [Required]
        public int DeviceId { get; set; }

        [Required]
        public string Type { get; set; } = string.Empty; 
 
        [Required]
        public string IdempotencyKey { get; set; } = string.Empty;
    }

    public class CommandDto
    {
        public int Id { get; set; }
        public int DeviceId { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string IdempotencyKey { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? AcknowledgedAt { get; set; }
    }
}