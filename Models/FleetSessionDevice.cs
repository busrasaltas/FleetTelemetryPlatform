namespace FleetTelemetryPlatform.Models
{
    public class FleetSessionDevice
    {
        public int FleetSessionId { get; set; }
        public FleetSession FleetSession { get; set; } = null!;

        public int DeviceId { get; set; }
        public Device Device { get; set; } = null!;

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    }
}