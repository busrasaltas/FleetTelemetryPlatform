using FleetTelemetryPlatform.DTOs;
using FleetTelemetryPlatform.Models;
using FleetTelemetryPlatform.Repositoriess;
using Microsoft.EntityFrameworkCore;

namespace FleetTelemetryPlatform.Services
{
    public class DeviceNotFoundException : Exception
    {
        public DeviceNotFoundException(int id) : base($"Device with id {id} not found.") { }
    }

    public class DeviceConcurrencyException : Exception
    {
        public DeviceConcurrencyException()
            : base("The device was modified by another user. Please refresh and try again.") { }
    }

    public class DeviceService : IDeviceService
    {
        private readonly IDeviceRepository _repository;

        public DeviceService(IDeviceRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<DeviceDto>> GetAllAsync()
        {
            var devices = await _repository.GetAllAsync();
            return devices.Select(MapToDto).ToList();
        }

        public async Task<DeviceDto?> GetByIdAsync(int id)
        {
            var device = await _repository.GetByIdAsync(id);
            return device is null ? null : MapToDto(device);
        }

        public async Task<DeviceDto> CreateAsync(CreateDeviceDto dto)
        {
            var device = new Device
            {
                Name = dto.Name,
                SerialNumber = dto.SerialNumber,
                Status = DeviceStatus.Offline
            };

            var created = await _repository.AddAsync(device);
            return MapToDto(created);
        }

        public async Task<DeviceDto> UpdateAsync(int id, UpdateDeviceDto dto)
        {
            var device = await _repository.GetByIdAsync(id)
                ?? throw new DeviceNotFoundException(id);

            device.Name = dto.Name;
            device.Status = Enum.Parse<DeviceStatus>(dto.Status);

            device.RowVersion = Convert.FromBase64String(dto.RowVersion);

            try
            {
                await _repository.UpdateAsync(device);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new DeviceConcurrencyException();
            }

            return MapToDto(device);
        }

        public async Task DeleteAsync(int id)
        {
            var device = await _repository.GetByIdAsync(id)
                ?? throw new DeviceNotFoundException(id);

            await _repository.DeleteAsync(device);
        }

        private static DeviceDto MapToDto(Device device) => new()
        {
            Id = device.Id,
            Name = device.Name,
            SerialNumber = device.SerialNumber,
            Status = device.Status.ToString(),
            LastSeenAt = device.LastSeenAt,
            RowVersion = Convert.ToBase64String(device.RowVersion)
        };
    }
}