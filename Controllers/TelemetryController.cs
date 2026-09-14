using Microsoft.AspNetCore.Mvc;
using FleetTelemetryPlatform.DTOs;
using FleetTelemetryPlatform.Messaging;
using FleetTelemetryPlatform.Messaging.Events;

namespace FleetTelemetryPlatform.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TelemetryController : ControllerBase
    {
        private readonly IMessagePublisher _publisher;
        private const string QueueName = "telemetry-received";

        public TelemetryController(IMessagePublisher publisher)
        {
            _publisher = publisher;
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
    }
}