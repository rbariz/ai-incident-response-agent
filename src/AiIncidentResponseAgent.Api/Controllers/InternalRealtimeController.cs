using AiIncidentResponseAgent.Api.Hubs;
using AiIncidentResponseAgent.Contracts.Realtime;
using AiIncidentResponseAgent.Infrastructure.Realtime;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;

namespace AiIncidentResponseAgent.Api.Controllers;

[AllowAnonymous]
[ApiController]
[Route("api/internal/realtime")]
public sealed class InternalRealtimeController : ControllerBase
{
    private readonly IHubContext<AgentHub> _hubContext;
    private readonly InternalApiKeyOptions _options;

    public InternalRealtimeController(IHubContext<AgentHub> hubContext, IOptions<InternalApiKeyOptions> options)
    {
        _hubContext = hubContext;
        _options = options.Value;
    }

    [HttpPost("broadcast")]
    public async Task<IActionResult> Broadcast(
        [FromBody] RealtimeEventRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.EventName))
        {
            return BadRequest("EventName is required.");
        }

        if (!Request.Headers.TryGetValue("X-Internal-Api-Key", out var providedKey) ||
                providedKey != _options.ApiKey)
        {
            return Unauthorized("Invalid internal API key.");
        }

        await _hubContext.Clients.All.SendAsync(
            request.EventName,
            request.Payload,
            cancellationToken);

        return Accepted();
    }
}
