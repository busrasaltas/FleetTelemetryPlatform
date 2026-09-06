namespace FleetTelemetryPlatform.Models
{
    public class Command
    {
        public int Id { get; set; }
        public int DeviceId { get; set; }
        public CommandType Type { get; set; }
        public CommandStatus Status { get; set; } = CommandStatus.Pending;
        public string IdempotencyKey { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? AcknowledgedAt { get; set; }

        public Device Device { get; set; } = null!;
    }

    public enum CommandType
    {
        ReturnToBase,
        ChangeAltitude,
        Pause,
        Resume
    }

    public enum CommandStatus
    {
        Pending,
        Sent,
        Acknowledged,
        Failed
    }
}