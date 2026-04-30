using AiIncidentResponseAgent.Domain.Autonomy;
using AiIncidentResponseAgent.Domain.Executions;
using AiIncidentResponseAgent.Domain.Incidents;

namespace AiIncidentResponseAgent.Application.Models
{
    public sealed class AgentDecisionResult
    {
        public AgentDecision Decision { get; init; }

        public AgentAction Action { get; init; }

        public AutonomyLevel AutonomyLevel { get; init; }

        public IncidentSeverity Severity { get; init; }

        public string Reason { get; init; } = string.Empty;

        public bool RequiresHumanApproval { get; init; }

    }


}
