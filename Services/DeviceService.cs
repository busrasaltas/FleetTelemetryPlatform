using FleetTelemetryPlatform.DTOs;
using FleetTelemetryPlatform.Models;
using FleetTelemetryPlatform.Repositoriess;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

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
        private readonly IDistributedCache _cache;
        private static readonly DistributedCacheEntryOptions CacheOptions = new()
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
        };

        public DeviceService(IDeviceRepository repository, IDistributedCache cache)
        {
            _repository = repository;
            _cache = cache;
        }

        public async Task<List<DeviceDto>> GetAllAsync()
        {
            var devices = await _repository.GetAllAsync();
            return devices.Select(MapToDto).ToList();
        }

        public async Task<DeviceDto?> GetByIdAsync(int id)
        {
            var cacheKey = GetCacheKey(id);

            var cached = await _cache.GetStringAsync(cacheKey);
            if (cached is not null)
            {
                return JsonSerializer.Deserialize<DeviceDto>(cached);
            }

            var device = await _repository.GetByIdAsync(id);
            if (device is null)
            {
                return null;
            }

            var dto = MapToDto(device);
            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(dto), CacheOptions);

            return dto;
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

            if (!Enum.TryParse<DeviceStatus>(dto.Status, out var status))
                throw new ArgumentException($"Invalid device status: '{dto.Status}'. Valid values: Offline, Online, InFlight.");

            device.Status = status;
            device.RowVersion = Convert.FromBase64String(dto.RowVersion);

            try
            {
                await _repository.UpdateAsync(device);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new DeviceConcurrencyException();
            }

            await _cache.RemoveAsync(GetCacheKey(id));

            return MapToDto(device);
        }

        public async Task DeleteAsync(int id)
        {
            var device = await _repository.GetByIdAsync(id)
                ?? throw new DeviceNotFoundException(id);

            await _repository.DeleteAsync(device);
            await _cache.RemoveAsync(GetCacheKey(id));
        }

        private static string GetCacheKey(int id) => $"device:{id}";

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