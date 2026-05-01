using AiIncidentResponseAgent.Application.Abstractions.Repositories;
using AiIncidentResponseAgent.Contracts.Ops;

using Microsoft.AspNetCore.Mvc;

namespace AiIncidentResponseAgent.Api.Controllers;

[ApiController]
[Route("api/agent-timeline")]
public sealed class AgentTimelineController : ControllerBase
{
    private readonly IAgentEventRepository _events;
    private readonly IAgentExecutionRepository _executions;
    private readonly IIncidentRepository _incidents;

    public AgentTimelineController(
        IAgentEventRepository events,
        IAgentExecutionRepository executions,
        IIncidentRepository incidents)
    {
        _events = events;
        _executions = executions;
        _incidents = incidents;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AgentTimelineItemResponse>>> Get(
    [FromQuery] int take = 300,
    CancellationToken cancellationToken = default)
    {
        take = Math.Clamp(take, 1, 500);

        var events = await _events.GetLatestAsync(take, cancellationToken);
        var executions = await _executions.GetLatestAsync(take, cancellationToken);
        var incidents = await _incidents.GetLatestAsync(take, cancellationToken);

        var items = new List<AgentTimelineItemResponse>();

        items.AddRange(events.Select(x => new AgentTimelineItemResponse
        {
            Id = x.Id,
            OccurredAtUtc = x.CreatedAtUtc,
            ItemType = "Event",
            Title = x.Type.ToString(),
            Status = x.Processed ? "Processed" : "Pending",
            CorrelationId = x.CorrelationId
        }));

        items.AddRange(executions.Select(x => new AgentTimelineItemResponse
        {
            Id = x.Id,
            OccurredAtUtc = x.CreatedAtUtc,
            ItemType = "Execution",
            Title = x.Action.ToString(),
            Status = x.Status.ToString(),
            CorrelationId = x.CorrelationId
        }));

        items.AddRange(incidents.Select(x => new AgentTimelineItemResponse
        {
            Id = x.Id,
            OccurredAtUtc = x.CreatedAtUtc,
            ItemType = "Incident",
            Title = x.Title,
            Status = x.Status.ToString(),
            CorrelationId = string.Empty
        }));

        return Ok(items
            .OrderByDescending(x => x.OccurredAtUtc)
            .Take(take)
            .ToList());
    }
}