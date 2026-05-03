namespace AiIncidentResponseAgent.Application.Abstractions.Repositories
{
    public interface IAuditService
    {
        Task WriteAsync(
            string actorType,
            string actorName,
            string action,
            string entityType,
            string entityId,
            string correlationId,
            string detailsJson = "{}",
            CancellationToken cancellationToken = default);
    }
}
