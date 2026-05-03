using AiIncidentResponseAgent.Application.Abstractions;
using AiIncidentResponseAgent.Application.Abstractions.Repositories;
using AiIncidentResponseAgent.Application.Models;
using AiIncidentResponseAgent.Domain.Autonomy;
using AiIncidentResponseAgent.Domain.Executions;

namespace AiIncidentResponseAgent.Application.Services;

public sealed class SafeAgentPolicyEngine : IAgentPolicyEngine
{
    private readonly IAgentActionLockRepository _locks;

    public SafeAgentPolicyEngine(IAgentActionLockRepository locks)
    {
        _locks = locks;
    }

    public async Task<PolicyCheckResult> CheckAsync(
        AgentContext context,
        AgentDecisionResult decision,
        CancellationToken cancellationToken = default)
    {
        if (decision.Action == AgentAction.None)
        {
            return PolicyCheckResult.Allow();
        }

        if (decision.RequiresHumanApproval)
        {
            return PolicyCheckResult.Deny("Human approval required.");
        }

        if (decision.AutonomyLevel == AutonomyLevel.Critical)
        {
            return PolicyCheckResult.Deny("Critical autonomous actions are blocked by default.");
        }

        if (decision.Action == AgentAction.RestartService)
        {
            return PolicyCheckResult.Deny("RestartService requires explicit manual approval.");
        }

        if (string.IsNullOrWhiteSpace(context.Event.CorrelationId))
        {
            return PolicyCheckResult.Deny(
                $"{decision.Action} requires a correlation id.");
        }

        var alreadyLocked = await _locks.ExistsAsync(
            decision.Action,
            context.Event.CorrelationId,
            cancellationToken);

        if (alreadyLocked)
        {
            return PolicyCheckResult.Deny(
                $"Action {decision.Action} already successfully executed for correlation id {context.Event.CorrelationId}.");
        }

        return PolicyCheckResult.Allow();
    }
}
