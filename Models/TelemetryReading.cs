namespace FleetTelemetryPlatform.Models
{
    public class TelemetryReading
    {
        public long Id { get; set; }
        public int DeviceId { get; set; }
        public DateTime Timestamp { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Altitude { get; set; }
        public double BatteryLevel { get; set; }
        public double Speed { get; set; }

        public Device Device { get; set; } = null!;
    }
}