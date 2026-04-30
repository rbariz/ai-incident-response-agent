using AiIncidentResponseAgent.Application.Abstractions;
using AiIncidentResponseAgent.Application.Models;
using AiIncidentResponseAgent.Domain.Autonomy;
using AiIncidentResponseAgent.Domain.Events;
using AiIncidentResponseAgent.Domain.Executions;
using AiIncidentResponseAgent.Domain.Incidents;

namespace AiIncidentResponseAgent.Application.Services;

public sealed class RuleBasedAgentDecisionEngine : IAgentDecisionEngine
{
    public Task<AgentDecisionResult> DecideAsync(
        AgentContext context,
        AgentAnalysisResult analysis,
        CancellationToken cancellationToken = default)
    {
        var result = context.Event.Type switch
        {
            AgentEventType.DuplicateScan => new AgentDecisionResult
            {
                Decision = AgentDecision.ExecuteAction,
                Action = AgentAction.BlockTicket,
                AutonomyLevel = AutonomyLevel.High,
                Severity = IncidentSeverity.High,
                Reason = "Duplicate scan detected. Ticket should be blocked.",
                RequiresHumanApproval = false
            },

            AgentEventType.ApiErrorSpike => new AgentDecisionResult
            {
                Decision = AgentDecision.ExecuteAndEscalate,
                Action = AgentAction.RestartService,
                AutonomyLevel = AutonomyLevel.Critical,
                Severity = IncidentSeverity.Critical,
                Reason = "API error spike detected. Service restart and escalation required.",
                RequiresHumanApproval = false
            },

            AgentEventType.FraudRiskDetected => new AgentDecisionResult
            {
                Decision = analysis.ConfidenceScore >= 0.80m
                    ? AgentDecision.ExecuteAction
                    : AgentDecision.SuggestAction,
                Action = AgentAction.CreateIncident,
                AutonomyLevel = analysis.ConfidenceScore >= 0.80m
                    ? AutonomyLevel.High
                    : AutonomyLevel.Medium,
                Severity = IncidentSeverity.High,
                Reason = "Fraud risk detected.",
                RequiresHumanApproval = analysis.ConfidenceScore < 0.80m
            },

            AgentEventType.SuspiciousBusinessActivity => new AgentDecisionResult
            {
                Decision = AgentDecision.SuggestAction,
                Action = AgentAction.Escalate,
                AutonomyLevel = AutonomyLevel.Medium,
                Severity = IncidentSeverity.Medium,
                Reason = "Suspicious business activity requires operator review.",
                RequiresHumanApproval = true
            },

            _ => new AgentDecisionResult
            {
                Decision = AgentDecision.ObserveOnly,
                Action = AgentAction.None,
                AutonomyLevel = AutonomyLevel.Low,
                Severity = IncidentSeverity.Low,
                Reason = "No automatic action configured for this event type.",
                RequiresHumanApproval = false
            }
        };

        return Task.FromResult(result);
    }
}
