using FleetTelemetryPlatform.Data;
using FleetTelemetryPlatform.DTOs;
using FleetTelemetryPlatform.Models;
using Microsoft.EntityFrameworkCore;

namespace FleetTelemetryPlatform.Services
{
    public class CommandService : ICommandService
    {
        private readonly AppDbContext _context;

        public CommandService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<CommandDto> IssueAsync(IssueCommandDto dto)
        { 
            var existing = await _context.Commands
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.IdempotencyKey == dto.IdempotencyKey);

            if (existing is not null)
            {
                return MapToDto(existing);
            }

            if (!Enum.TryParse<CommandType>(dto.Type, out var commandType))
            {
                throw new ArgumentException($"Invalid command type: {dto.Type}");
            }

            var command = new Command
            {
                DeviceId = dto.DeviceId,
                Type = commandType,
                Status = CommandStatus.Pending,
                IdempotencyKey = dto.IdempotencyKey,
                CreatedAt = DateTime.UtcNow
            };

            _context.Commands.Add(command);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
            {
                var winner = await _context.Commands
                    .AsNoTracking()
                    .FirstAsync(c => c.IdempotencyKey == dto.IdempotencyKey);

                return MapToDto(winner);
            }

            return MapToDto(command);
        }

        private static bool IsUniqueConstraintViolation(DbUpdateException ex)
        {
            return ex.InnerException?.Message.Contains("IX_Commands_IdempotencyKey") == true
                || ex.InnerException?.Message.Contains("unique") == true;
        }

        private static CommandDto MapToDto(Command command) => new()
        {
            Id = command.Id,
            DeviceId = command.DeviceId,
            Type = command.Type.ToString(),
            Status = command.Status.ToString(),
            IdempotencyKey = command.IdempotencyKey,
            CreatedAt = command.CreatedAt,
            AcknowledgedAt = command.AcknowledgedAt
        };
    }
}