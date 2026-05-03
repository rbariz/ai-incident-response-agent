using AiIncidentResponseAgent.Api.Hubs;
using AiIncidentResponseAgent.Application.Abstractions;

using Microsoft.AspNetCore.SignalR;

namespace AiIncidentResponseAgent.Api.Realtime;

public sealed class SignalRRealtimeNotifier : IRealtimeNotifier
{
    private readonly IHubContext<AgentHub> _hubContext;

    public SignalRRealtimeNotifier(IHubContext<AgentHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public Task AgentEventCreatedAsync(
        Guid eventId,
        string correlationId,
        CancellationToken cancellationToken = default)
    {
        return _hubContext.Clients.All.SendAsync(
            "AgentEventCreated",
            new
            {
                eventId,
                correlationId
            },
            cancellationToken);
    }

    public Task AgentExecutionStartedAsync(
        Guid executionId,
        Guid eventId,
        string correlationId,
        CancellationToken cancellationToken = default)
    {
        return _hubContext.Clients.All.SendAsync(
            "AgentExecutionStarted",
            new
            {
                executionId,
                eventId,
                correlationId
            },
            cancellationToken);
    }

    public Task AgentExecutionCompletedAsync(
        Guid executionId,
        Guid eventId,
        string status,
        string action,
        string correlationId,
        CancellationToken cancellationToken = default)
    {
        return _hubContext.Clients.All.SendAsync(
            "AgentExecutionCompleted",
            new
            {
                executionId,
                eventId,
                status,
                action,
                correlationId
            },
            cancellationToken);
    }

    public Task IncidentChangedAsync(
        Guid incidentId,
        Guid eventId,
        string status,
        string severity,
        CancellationToken cancellationToken = default)
    {
        return _hubContext.Clients.All.SendAsync(
            "IncidentChanged",
            new
            {
                incidentId,
                eventId,
                status,
                severity
            },
            cancellationToken);
    }

    public Task AgentExecutionApprovalChangedAsync(
    Guid executionId,
    string status,
    string reason,
    CancellationToken cancellationToken = default)
    {
        return _hubContext.Clients.All.SendAsync(
            "AgentExecutionApprovalChanged",
            new
            {
                executionId,
                status,
                reason
            },
            cancellationToken);
    }
}