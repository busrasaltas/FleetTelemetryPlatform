using Microsoft.AspNetCore.Mvc;
using FleetTelemetryPlatform.DTOs;
using FleetTelemetryPlatform.Services;

namespace FleetTelemetryPlatform.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommandsController : ControllerBase
    {
        private readonly ICommandService _commandService;

        public CommandsController(ICommandService commandService)
        {
            _commandService = commandService;
        }

        [HttpPost]
        public async Task<ActionResult<CommandDto>> Issue(IssueCommandDto dto)
        {
            var result = await _commandService.IssueAsync(dto);
            return Ok(result);
        }
    }
}