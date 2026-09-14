using System.ComponentModel.DataAnnotations;

namespace FleetTelemetryPlatform.DTOs
{
    public class SubmitTelemetryDto
    {
        [Required]
        public int DeviceId { get; set; }

        [Range(-90, 90)]
        public double Latitude { get; set; }

        [Range(-180, 180)]
        public double Longitude { get; set; }

        public double Altitude { get; set; }

        [Range(0, 100)]
        public double BatteryLevel { get; set; }

        public double Speed { get; set; }
    }
}