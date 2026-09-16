using FleetTelemetryPlatform.DTOs;
using FleetTelemetryPlatform.Data;
using FleetTelemetryPlatform.Messaging;
using FleetTelemetryPlatform.Messaging.Events;
using FleetTelemetryPlatform.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FleetTelemetryPlatform.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TelemetryController : ControllerBase
    {
        private readonly IMessagePublisher _publisher;
        private readonly AppDbContext _context;
        private const string QueueName = "telemetry-received";

        public TelemetryController(IMessagePublisher publisher, AppDbContext context)
        {
            _publisher = publisher;
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Ingest(SubmitTelemetryDto dto)
        {
            var telemetryEvent = new TelemetryReceivedEvent
            {
                DeviceId = dto.DeviceId,
                Timestamp = DateTime.UtcNow,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                Altitude = dto.Altitude,
                BatteryLevel = dto.BatteryLevel,
                Speed = dto.Speed
            };

            await _publisher.PublishAsync(telemetryEvent, QueueName);

            return Accepted();
        }

        [HttpGet("device/{deviceId}")]
        public async Task<ActionResult<List<TelemetryReading>>> GetByDevice(int deviceId)
        {
            var readings = await _context.TelemetryReadings
                .AsNoTracking()
                .Where(t => t.DeviceId == deviceId)
                .OrderByDescending(t => t.Timestamp)
                .Take(50)
                .ToListAsync();

            return Ok(readings);
        }
    }
}