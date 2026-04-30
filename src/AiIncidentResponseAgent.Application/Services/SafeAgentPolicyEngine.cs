using AiIncidentResponseAgent.Application.Abstractions;
using AiIncidentResponseAgent.Application.Abstractions.Repositories;
using AiIncidentResponseAgent.Application.Models;
using AiIncidentResponseAgent.Domain.Autonomy;
using AiIncidentResponseAgent.Domain.Executions;

namespace AiIncidentResponseAgent.Application.Services;

public sealed class SafeAgentPolicyEngine : IAgentPolicyEngine
{
    //private readonly IAgentExecutionRepository _executions;
    //
    //public SafeAgentPolicyEngine(IAgentExecutionRepository executions)
    //{
    //    _executions = executions;
    //}
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
            return PolicyCheckResult.Allow();

        if (decision.RequiresHumanApproval)
            return PolicyCheckResult.Deny("Human approval required.");

        if (decision.AutonomyLevel == AutonomyLevel.Critical)
            return PolicyCheckResult.Deny("Critical autonomous actions are blocked by default.");

        if (decision.Action == AgentAction.RestartService)
            return PolicyCheckResult.Deny("RestartService requires explicit manual approval.");

        if (decision.Action == AgentAction.BlockTicket &&
            string.IsNullOrWhiteSpace(context.Event.CorrelationId))
            return PolicyCheckResult.Deny("BlockTicket requires a correlation id.");

        var alreadyExecutedKey =
            $"action:{decision.Action}:correlation:{context.Event.CorrelationId}";

        var existing = await _locks.ExistsAsync(
                decision.Action,
                context.Event.CorrelationId,
                cancellationToken);

        if (existing)
            return PolicyCheckResult.Deny($"Action {decision.Action} already executed for this correlation id.");

        return PolicyCheckResult.Allow();
    }
}
