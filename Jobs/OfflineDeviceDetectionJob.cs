using FleetTelemetryPlatform.Data;
using FleetTelemetryPlatform.Models;
using Microsoft.EntityFrameworkCore;
using FleetTelemetryPlatform.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace FleetTelemetryPlatform.Jobs
{
    public class OfflineDeviceDetectionJob
    {
        private readonly AppDbContext _context;
        private static readonly TimeSpan OfflineThreshold = TimeSpan.FromMinutes(5);
        private readonly IHubContext<DeviceStatusHub> _hubContext;

        public OfflineDeviceDetectionJob(
            AppDbContext context,
            IHubContext<DeviceStatusHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public async Task ExecuteAsync()
        {
            var cutoff = DateTime.UtcNow - OfflineThreshold;

            var affectedDeviceIds = await _context.Devices
                .Where(d => d.Status != DeviceStatus.Offline &&
                            (d.LastSeenAt == null || d.LastSeenAt < cutoff))
                .Select(d => d.Id)
                .ToListAsync();

            if (affectedDeviceIds.Count == 0)
            {
                return;
            }

            await _context.Devices
                .Where(d => affectedDeviceIds.Contains(d.Id))
                .ExecuteUpdateAsync(setters => setters.SetProperty(d => d.Status, DeviceStatus.Offline));

            foreach (var deviceId in affectedDeviceIds)
            {
                await _hubContext.Clients.All.SendAsync("DeviceStatusChanged", new DeviceStatusUpdate
                {
                    DeviceId = deviceId,
                    Status = "Offline",
                    LastSeenAt = null
                });
            }

            Console.WriteLine($"[OfflineDeviceDetectionJob] Marked {affectedDeviceIds.Count} device(s) as Offline.");
        }
    }
}
