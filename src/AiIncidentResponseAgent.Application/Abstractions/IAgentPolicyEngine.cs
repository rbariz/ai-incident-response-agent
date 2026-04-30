using AiIncidentResponseAgent.Application.Models;

namespace AiIncidentResponseAgent.Application.Abstractions
{
    public interface IAgentPolicyEngine
    {
        Task<PolicyCheckResult> CheckAsync(
            AgentContext context,
            AgentDecisionResult decision,
            CancellationToken cancellationToken = default);
    }
}
