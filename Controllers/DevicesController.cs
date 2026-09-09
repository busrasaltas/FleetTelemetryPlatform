using Microsoft.AspNetCore.Mvc;
using FleetTelemetryPlatform.DTOs;
using FleetTelemetryPlatform.Services;

namespace FleetTelemetryPlatform.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DevicesController : ControllerBase
    {
        private readonly IDeviceService _deviceService;

        public DevicesController(IDeviceService deviceService)
        {
            _deviceService = deviceService;
        }

        [HttpGet]
        public async Task<ActionResult<List<DeviceDto>>> GetAll()
        {
            var devices = await _deviceService.GetAllAsync();
            return Ok(devices);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DeviceDto>> GetById(int id)
        {
            var device = await _deviceService.GetByIdAsync(id);
            if (device is null)
                return NotFound();

            return Ok(device);
        }

        [HttpPost]
        public async Task<ActionResult<DeviceDto>> Create(CreateDeviceDto dto)
        {
            var created = await _deviceService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<DeviceDto>> Update(int id, UpdateDeviceDto dto)
        {
            var updated = await _deviceService.UpdateAsync(id, dto);
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _deviceService.DeleteAsync(id);
            return NoContent();
        }
    }
}