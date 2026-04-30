using AiIncidentResponseAgent.Application.Models;

namespace AiIncidentResponseAgent.Application.Abstractions
{
    public interface IAgentFeedbackHandler
    {
        Task HandleAsync(
            AgentContext context,
            AgentDecisionResult decision,
            AgentActionResult actionResult,
            CancellationToken cancellationToken = default);
    }
}
