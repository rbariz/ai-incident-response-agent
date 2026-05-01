using AiIncidentResponseAgent.Application.Abstractions.Repositories;
using AiIncidentResponseAgent.Contracts.Ops;
using AiIncidentResponseAgent.Domain.Incidents;

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
    public async Task<ActionResult<IReadOnlyList<IncidentResponse>>> GetLatest(
        [FromQuery] string? status,
        [FromQuery] int take = 200,
        CancellationToken cancellationToken = default)
    {
        take = Math.Clamp(take, 1, 200);

        var incidents = await _incidents.GetLatestByStatusAsync(
            status,
            take,
            cancellationToken);

        return Ok(incidents.Select(ToResponse).ToList());
    }

    private static IncidentResponse ToResponse(Incident x) => new()
    {
        Id = x.Id,
        AgentEventId = x.AgentEventId,
        Title = x.Title,
        Description = x.Description,
        Severity = x.Severity.ToString(),
        Status = x.Status.ToString(),
        CreatedAtUtc = x.CreatedAtUtc,
        ResolvedAtUtc = x.ResolvedAtUtc
    };
}
