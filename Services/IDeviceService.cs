using FleetTelemetryPlatform.DTOs;

namespace FleetTelemetryPlatform.Services
{
    public interface IDeviceService
    {
        Task<List<DeviceDto>> GetAllAsync();
        Task<DeviceDto?> GetByIdAsync(int id);
        Task<DeviceDto> CreateAsync(CreateDeviceDto dto);
        Task<DeviceDto> UpdateAsync(int id, UpdateDeviceDto dto);
        Task DeleteAsync(int id);
    }
}