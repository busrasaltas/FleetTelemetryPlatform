using FleetTelemetryPlatform.Models;

namespace FleetTelemetryPlatform.Repositoriess
{
    public interface IDeviceRepository
{
    Task<List<Device>> GetAllAsync();
    Task<Device?> GetByIdAsync(int id);
    Task<Device> AddAsync(Device device);
    Task UpdateAsync(Device device);
    Task DeleteAsync(Device device);
}
}