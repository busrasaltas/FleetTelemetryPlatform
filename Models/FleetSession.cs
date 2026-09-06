namespace FleetTelemetryPlatform.Models
{
    public class FleetSession
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime StartedAt { get; set; } = DateTime.UtcNow;
        public DateTime? EndedAt { get; set; }
        public FleetSessionStatus Status { get; set; } = FleetSessionStatus.Active;

        public ICollection<FleetSessionDevice> FleetSessionDevices { get; set; } = new List<FleetSessionDevice>();
    }

    public enum FleetSessionStatus
    {
        Active,
        Completed
    }
}