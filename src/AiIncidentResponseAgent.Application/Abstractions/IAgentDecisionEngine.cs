using AiIncidentResponseAgent.Application.Models;

namespace AiIncidentResponseAgent.Application.Abstractions
{
    public interface IAgentDecisionEngine
    {
        Task<AgentDecisionResult> DecideAsync(
            AgentContext context,
            AgentAnalysisResult analysis,
            CancellationToken cancellationToken = default);
    }
}
