using AiIncidentResponseAgent.Application.Abstractions.Repositories;
using AiIncidentResponseAgent.Contracts.Common;
using AiIncidentResponseAgent.Domain.Audit;
using AiIncidentResponseAgent.Infrastructure.Persistence.Paging;

namespace AiIncidentResponseAgent.Infrastructure.Persistence.Repositories;

public sealed class AuditLogRepository : IAuditLogRepository
{
    private readonly AgentDbContext _db;

    public AuditLogRepository(AgentDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(
        AuditLog auditLog,
        CancellationToken cancellationToken = default)
    {
        await _db.AuditLogs.AddAsync(auditLog, cancellationToken);
    }

    public async Task<PagedResponse<AuditLog>> GetPagedAsync(
        string? entityType,
        string? entityId,
        string? correlationId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _db.AuditLogs.AsQueryable();

        if (!string.IsNullOrWhiteSpace(entityType))
        {
            query = query.Where(x => x.EntityType == entityType);
        }

        if (!string.IsNullOrWhiteSpace(entityId))
        {
            query = query.Where(x => x.EntityId == entityId);
        }

        if (!string.IsNullOrWhiteSpace(correlationId))
        {
            query = query.Where(x => x.CorrelationId == correlationId);
        }

        return await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToPagedResponseAsync(page, pageSize, cancellationToken);
    }
}