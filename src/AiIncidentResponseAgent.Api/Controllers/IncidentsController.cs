using AiIncidentResponseAgent.Application.Abstractions.Repositories;

using Microsoft.AspNetCore.Mvc;

namespace AiIncidentResponseAgent.Api.Controllers;

[ApiController]
[Route("api/incidents")]
public sealed class IncidentsController : ControllerBase
{
    private readonly IIncidentRepository _incidents;

    public IncidentsController(IIncidentRepository incidents)
    {
        _incidents = incidents;
    }

    [HttpGet]
    public async Task<IActionResult> GetLatest(
        [FromQuery] int take = 50,
        CancellationToken cancellationToken = default)
    {
        take = Math.Clamp(take, 1, 200);

        var incidents = await _incidents.GetLatestAsync(take, cancellationToken);

        return Ok(incidents.Select(x => new
        {
            x.Id,
            x.AgentEventId,
            x.Title,
            x.Description,
            x.Severity,
            x.Status,
            x.CreatedAtUtc,
            x.ResolvedAtUtc
        }));
    }
}