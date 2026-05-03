using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AiIncidentResponseAgent.Application.Abstractions;

namespace AiIncidentResponseAgent.Infrastructure.Realtime;
public sealed class NoOpRealtimeNotifier : IRealtimeNotifier
{
    public Task AgentEventCreatedAsync(Guid eventId, string correlationId, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task AgentExecutionStartedAsync(Guid executionId, Guid eventId, string correlationId, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task AgentExecutionCompletedAsync(Guid executionId, Guid eventId, string status, string action, string correlationId, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task IncidentChangedAsync(Guid incidentId, Guid eventId, string status, string severity, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task AgentExecutionApprovalChangedAsync(Guid executionId, string status, string reason, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
