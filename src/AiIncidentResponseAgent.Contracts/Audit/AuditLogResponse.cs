using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiIncidentResponseAgent.Contracts.Audit;
public sealed class AuditLogResponse
{
    public Guid Id { get; set; }
    public string ActorType { get; set; } = string.Empty;
    public string ActorName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string CorrelationId { get; set; } = string.Empty;
    public string DetailsJson { get; set; } = "{}";
    public DateTime CreatedAtUtc { get; set; }
}
