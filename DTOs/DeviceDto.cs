using System.ComponentModel.DataAnnotations;

namespace FleetTelemetryPlatform.DTOs
{
    public class DeviceDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? LastSeenAt { get; set; }
        public string RowVersion { get; set; } = string.Empty;
    }

    public class CreateDeviceDto
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string SerialNumber { get; set; } = string.Empty;
    }

    public class UpdateDeviceDto
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Status { get; set; } = string.Empty;

        [Required]
        public string RowVersion { get; set; } = string.Empty;
    }
}