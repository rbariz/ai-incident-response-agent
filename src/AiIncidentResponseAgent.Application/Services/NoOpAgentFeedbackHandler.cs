using AiIncidentResponseAgent.Application.Abstractions;
using AiIncidentResponseAgent.Application.Models;

namespace AiIncidentResponseAgent.Application.Services;

public sealed class NoOpAgentFeedbackHandler : IAgentFeedbackHandler
{
    public Task HandleAsync(
        AgentContext context,
        AgentDecisionResult decision,
        AgentActionResult actionResult,
        CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
