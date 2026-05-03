using AiIncidentResponseAgent.Application.Abstractions;
using AiIncidentResponseAgent.Application.Abstractions.Repositories;
using AiIncidentResponseAgent.Contracts.AgentEvents;
using AiIncidentResponseAgent.Contracts.Common;
using AiIncidentResponseAgent.Contracts.Ops;
using AiIncidentResponseAgent.Domain.Events;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AiIncidentResponseAgent.Api.Controllers;


[Authorize(Policy = "CanViewOps")]
[ApiController]
[Route("api/agent-events")]
public sealed class AgentEventsController : ControllerBase
{
    private readonly IAgentEventRepository _events;
    private readonly IAgentOrchestrator _orchestrator;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRealtimeNotifier _realtime;

    public AgentEventsController(
        IAgentEventRepository events,
        IAgentOrchestrator orchestrator,
        IUnitOfWork unitOfWork,
        IRealtimeNotifier realtime)
    {
        _events = events;
        _orchestrator = orchestrator;
        _unitOfWork = unitOfWork;
        _realtime = realtime;
    }

    [HttpPost]
    [Authorize(Policy = "CanManageTickets")]
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
                correlationId,
                request.Lang);

        await _events.AddAsync(agentEvent, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _realtime.AgentEventCreatedAsync(
    agentEvent.Id,
    agentEvent.CorrelationId,
    cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = agentEvent.Id },
            ToResponse(agentEvent));
    }

    [HttpPost("{id:guid}/process")]
    [Authorize(Policy = "CanManageApprovals")]
    public async Task<IActionResult> Process(
        Guid id,
        CancellationToken cancellationToken)
    {
        await _orchestrator.ProcessEventAsync(id, cancellationToken);
        return Accepted();
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "CanViewOps")]
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
    [Authorize(Policy = "CanViewOps")]
    public async Task<ActionResult<PagedResponse<AgentEventResponse>>> GetLatest(
    [FromQuery] string? correlationId,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 50,
    CancellationToken cancellationToken = default)
    {
        var events = await _events.GetPagedAsync(
            page,
            pageSize,
            cancellationToken);

        return Ok(new PagedResponse<AgentEventResponse>
        {
            Items = events.Items.Select(ToResponse).ToList(),
            Page = events.Page,
            PageSize = events.PageSize,
            TotalCount = events.TotalCount
        });
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
            ProcessedAtUtc = agentEvent.ProcessedAtUtc,
            Lang = agentEvent.Lang
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
