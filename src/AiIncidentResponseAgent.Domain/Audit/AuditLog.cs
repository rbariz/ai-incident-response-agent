using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AiIncidentResponseAgent.Domain.Common;

namespace AiIncidentResponseAgent.Domain.Audit;

public sealed class AuditLog : Entity
{
    private AuditLog() { }

    public AuditLog(
        string actorType,
        string actorName,
        string action,
        string entityType,
        string entityId,
        string correlationId,
        string detailsJson)
    {
        ActorType = actorType;
        ActorName = actorName;
        Action = action;
        EntityType = entityType;
        EntityId = entityId;
        CorrelationId = correlationId;
        DetailsJson = detailsJson;
    }

    public string ActorType { get; private set; } = string.Empty; // System / User / Worker / Agent
    public string ActorName { get; private set; } = string.Empty;
    public string Action { get; private set; } = string.Empty;
    public string EntityType { get; private set; } = string.Empty;
    public string EntityId { get; private set; } = string.Empty;
    public string CorrelationId { get; private set; } = string.Empty;
    public string DetailsJson { get; private set; } = "{}";
}
