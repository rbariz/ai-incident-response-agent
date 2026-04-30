using AiIncidentResponseAgent.Application.Abstractions;
using AiIncidentResponseAgent.Application.Abstractions.Repositories;
using AiIncidentResponseAgent.Contracts.AgentEvents;
using AiIncidentResponseAgent.Domain.Events;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AiIncidentResponseAgent.Api.Controllers;


[ApiController]
[Route("api/agent-events")]
public sealed class AgentEventsController : ControllerBase
{
    private readonly IAgentEventRepository _events;
    private readonly IAgentOrchestrator _orchestrator;
    private readonly IUnitOfWork _unitOfWork;

    public AgentEventsController(
        IAgentEventRepository events,
        IAgentOrchestrator orchestrator,
        IUnitOfWork unitOfWork)
    {
        _events = events;
        _orchestrator = orchestrator;
        _unitOfWork = unitOfWork;
    }

    [HttpPost]
    public async Task<ActionResult<AgentEventResponse>> Create(
        [FromBody] CreateAgentEventRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Source))
        {
            return BadRequest("Source is required.");
        }

        if (string.IsNullOrWhiteSpace(request.PayloadJson))
        {
            request.PayloadJson = "{}";
        }

        var correlationId = string.IsNullOrWhiteSpace(request.CorrelationId)
            ? Guid.NewGuid().ToString("N")
            : request.CorrelationId;

        var agentEvent = new AgentEvent(
                MapType(request.Type),
                request.Source.Trim(),
                request.PayloadJson,
                correlationId);

        await _events.AddAsync(agentEvent, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = agentEvent.Id },
            ToResponse(agentEvent));
    }

    [HttpPost("{id:guid}/process")]
    public async Task<IActionResult> Process(
        Guid id,
        CancellationToken cancellationToken)
    {
        await _orchestrator.ProcessEventAsync(id, cancellationToken);
        return Accepted();
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AgentEventResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var agentEvent = await _events.GetByIdAsync(id, cancellationToken);

        if (agentEvent is null)
        {
            return NotFound();
        }

        return Ok(ToResponse(agentEvent));
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AgentEventResponse>>> GetLatest(
        [FromQuery] int take = 50,
        CancellationToken cancellationToken = default)
    {
        take = Math.Clamp(take, 1, 200);

        var events = await _events.GetLatestAsync(take, cancellationToken);

        return Ok(events.Select(ToResponse).ToList());
    }

    private static AgentEventResponse ToResponse(AgentEvent agentEvent)
    {
        return new AgentEventResponse
        {
            Id = agentEvent.Id,
            //Type = agentEvent.Type,
            Type = (int)agentEvent.Type,
            TypeName = agentEvent.Type.ToString(),
            Source = agentEvent.Source,
            PayloadJson = agentEvent.PayloadJson,
            CorrelationId = agentEvent.CorrelationId,
            Processed = agentEvent.Processed,
            CreatedAtUtc = agentEvent.CreatedAtUtc,
            ProcessedAtUtc = agentEvent.ProcessedAtUtc
        };
    }

    private static AgentEventType MapType(AgentEventTypeDto type)
    {
        return type switch
        {
            AgentEventTypeDto.DuplicateScan => AgentEventType.DuplicateScan,
            AgentEventTypeDto.FraudRiskDetected => AgentEventType.FraudRiskDetected,
            AgentEventTypeDto.ApiErrorSpike => AgentEventType.ApiErrorSpike,
            AgentEventTypeDto.SystemMetricAlert => AgentEventType.SystemMetricAlert,
            AgentEventTypeDto.SuspiciousBusinessActivity => AgentEventType.SuspiciousBusinessActivity,
            _ => AgentEventType.Unknown
        };
    }
}
