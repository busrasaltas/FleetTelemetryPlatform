namespace FleetTelemetryPlatform.Models
{
    public class Device
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;
        public DeviceStatus Status { get; set; }
        public DateTime? LastSeenAt { get; set; }

        // Navigation properties
        public ICollection<TelemetryReading> TelemetryReadings { get; set; } = new List<TelemetryReading>();
        public ICollection<Command> Commands { get; set; } = new List<Command>();
        public ICollection<FleetSessionDevice> FleetSessionDevices { get; set; } = new List<FleetSessionDevice>();
    }

    public enum DeviceStatus
    {
        Offline,
        Online,
        InFlight
    }
}