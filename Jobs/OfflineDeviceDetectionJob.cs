using FleetTelemetryPlatform.Data;
using FleetTelemetryPlatform.Models;
using Microsoft.EntityFrameworkCore;

namespace FleetTelemetryPlatform.Jobs
{
    public class OfflineDeviceDetectionJob
    {
        private readonly AppDbContext _context;
        private static readonly TimeSpan OfflineThreshold = TimeSpan.FromMinutes(5);

        public OfflineDeviceDetectionJob(AppDbContext context)
        {
            _context = context;
        }

        public async Task ExecuteAsync()
        {
            var cutoff = DateTime.UtcNow - OfflineThreshold;

            var affected = await _context.Devices
                .Where(d => d.Status != DeviceStatus.Offline &&
                            (d.LastSeenAt == null || d.LastSeenAt < cutoff))
                .ExecuteUpdateAsync(setters => setters.SetProperty(d => d.Status, DeviceStatus.Offline));

            if (affected > 0)
            {
                Console.WriteLine($"[OfflineDeviceDetectionJob] Marked {affected} device(s) as Offline.");
            }
        }
    }
}