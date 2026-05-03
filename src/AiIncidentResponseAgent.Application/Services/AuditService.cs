using AiIncidentResponseAgent.Application.Abstractions;
using AiIncidentResponseAgent.Application.Abstractions.Repositories;
using AiIncidentResponseAgent.Domain.Audit;

namespace AiIncidentResponseAgent.Application.Services;

public sealed class AuditService : IAuditService
{
    private readonly IAuditLogRepository _auditLogs;
    private readonly IUnitOfWork _unitOfWork;

    public AuditService(
        IAuditLogRepository auditLogs,
        IUnitOfWork unitOfWork)
    {
        _auditLogs = auditLogs;
        _unitOfWork = unitOfWork;
    }

    public async Task WriteAsync(
        string actorType,
        string actorName,
        string action,
        string entityType,
        string entityId,
        string correlationId,
        string detailsJson = "{}",
        CancellationToken cancellationToken = default)
    {
        var auditLog = new AuditLog(
            actorType,
            actorName,
            action,
            entityType,
            entityId,
            correlationId,
            detailsJson);

        await _auditLogs.AddAsync(auditLog, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}