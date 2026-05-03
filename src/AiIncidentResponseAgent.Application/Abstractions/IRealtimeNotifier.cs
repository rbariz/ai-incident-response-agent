namespace AiIncidentResponseAgent.Application.Abstractions
{
    public interface IRealtimeNotifier
    {
        Task AgentEventCreatedAsync(Guid eventId, string correlationId, CancellationToken cancellationToken = default);

        Task AgentExecutionStartedAsync(Guid executionId, Guid eventId, string correlationId, CancellationToken cancellationToken = default);

        Task AgentExecutionCompletedAsync(Guid executionId, Guid eventId, string status, string action, string correlationId, CancellationToken cancellationToken = default);

        Task IncidentChangedAsync(Guid incidentId, Guid eventId, string status, string severity, CancellationToken cancellationToken = default);

        Task AgentExecutionApprovalChangedAsync(
    Guid executionId,
    string status,
    string reason,
    CancellationToken cancellationToken = default);
    }
}
