using AiIncidentResponseAgent.Application.Abstractions.Repositories;
using AiIncidentResponseAgent.Contracts.Audit;
using AiIncidentResponseAgent.Contracts.Common;
using AiIncidentResponseAgent.Domain.Audit;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiIncidentResponseAgent.Api.Controllers;

[ApiController]
[Route("api/audit-logs")]
[Authorize(Policy = "CanViewOps")]
public sealed class AuditLogsController : ControllerBase
{
    private readonly IAuditLogRepository _auditLogs;

    public AuditLogsController(IAuditLogRepository auditLogs)
    {
        _auditLogs = auditLogs;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResponse<AuditLogResponse>>> GetPaged(
        [FromQuery] string? entityType,
        [FromQuery] string? entityId,
        [FromQuery] string? correlationId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var result = await _auditLogs.GetPagedAsync(
            entityType,
            entityId,
            correlationId,
            page,
            pageSize,
            cancellationToken);

        return Ok(new PagedResponse<AuditLogResponse>
        {
            Items = result.Items.Select(ToResponse).ToList(),
            Page = result.Page,
            PageSize = result.PageSize,
            TotalCount = result.TotalCount
        });
    }

    private static AuditLogResponse ToResponse(AuditLog x) => new()
    {
        Id = x.Id,
        ActorType = x.ActorType,
        ActorName = x.ActorName,
        Action = x.Action,
        EntityType = x.EntityType,
        EntityId = x.EntityId,
        CorrelationId = x.CorrelationId,
        DetailsJson = x.DetailsJson,
        CreatedAtUtc = x.CreatedAtUtc
    };
}
