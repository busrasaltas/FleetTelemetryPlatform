namespace FleetTelemetryPlatform.Messaging.Events
{
    public class TelemetryReceivedEvent
    {
        public int DeviceId { get; set; }
        public DateTime Timestamp { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Altitude { get; set; }
        public double BatteryLevel { get; set; }
        public double Speed { get; set; }
    }
}