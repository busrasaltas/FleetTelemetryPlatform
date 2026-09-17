using FleetTelemetryPlatform.DTOs;

namespace FleetTelemetryPlatform.Services
{
    public interface ICommandService
    {
        Task<CommandDto> IssueAsync(IssueCommandDto dto);
    }
}
