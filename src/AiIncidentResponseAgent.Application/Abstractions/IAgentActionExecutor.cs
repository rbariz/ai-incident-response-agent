using AiIncidentResponseAgent.Application.Models;
using AiIncidentResponseAgent.Domain.Executions;

namespace AiIncidentResponseAgent.Application.Abstractions
{
    public interface IAgentActionExecutor
    {
        Task<AgentActionResult> ExecuteAsync(
            AgentAction action,
            AgentContext context,
            CancellationToken cancellationToken = default);
    }
}
