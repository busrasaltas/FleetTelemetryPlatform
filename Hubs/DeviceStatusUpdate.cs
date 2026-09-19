namespace FleetTelemetryPlatform.Hubs
{
    public class DeviceStatusUpdate
    {
        public int DeviceId { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? LastSeenAt { get; set; }
    }
}
