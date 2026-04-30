using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AiIncidentResponseAgent.Domain.Common;
using AiIncidentResponseAgent.Domain.Executions;

namespace AiIncidentResponseAgent.Domain.Actions;
public sealed class AgentActionLock : Entity
{
    private AgentActionLock() { }

    public AgentActionLock(
        AgentAction action,
        string correlationId,
        Guid agentEventId)
    {
        Action = action;
        CorrelationId = correlationId;
        AgentEventId = agentEventId;
        LockedAtUtc = DateTime.UtcNow;
    }

    public AgentAction Action { get; private set; }
    public string CorrelationId { get; private set; } = string.Empty;
    public Guid AgentEventId { get; private set; }
    public DateTime LockedAtUtc { get; private set; }
}
