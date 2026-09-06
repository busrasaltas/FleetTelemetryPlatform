using Microsoft.EntityFrameworkCore;
using FleetTelemetryPlatform.Models;

namespace FleetTelemetryPlatform.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Device> Devices => Set<Device>();
        public DbSet<TelemetryReading> TelemetryReadings => Set<TelemetryReading>();
        public DbSet<Command> Commands => Set<Command>();
        public DbSet<FleetSession> FleetSessions => Set<FleetSession>();
        public DbSet<FleetSessionDevice> FleetSessionDevices => Set<FleetSessionDevice>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Composite key for the join entity
            modelBuilder.Entity<FleetSessionDevice>()
                .HasKey(fsd => new { fsd.FleetSessionId, fsd.DeviceId });

            modelBuilder.Entity<FleetSessionDevice>()
                .HasOne(fsd => fsd.FleetSession)
                .WithMany(fs => fs.FleetSessionDevices)
                .HasForeignKey(fsd => fsd.FleetSessionId);

            modelBuilder.Entity<FleetSessionDevice>()
                .HasOne(fsd => fsd.Device)
                .WithMany(d => d.FleetSessionDevices)
                .HasForeignKey(fsd => fsd.DeviceId);

            // Idempotency guarantee at DB level
            modelBuilder.Entity<Command>()
                .HasIndex(c => c.IdempotencyKey)
                .IsUnique();

            // Common query pattern: get latest readings for a device fast
            modelBuilder.Entity<TelemetryReading>()
                .HasIndex(t => new { t.DeviceId, t.Timestamp });
        }
    }
}