using AiIncidentResponseAgent.Contracts.Common;
using AiIncidentResponseAgent.Domain.Audit;

namespace AiIncidentResponseAgent.Application.Abstractions.Repositories
{
    public interface IAuditLogRepository
    {
        Task AddAsync(AuditLog auditLog, CancellationToken cancellationToken = default);

        Task<PagedResponse<AuditLog>> GetPagedAsync(
            string? entityType,
            string? entityId,
            string? correlationId,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default);
    }
}
