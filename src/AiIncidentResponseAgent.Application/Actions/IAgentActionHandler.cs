using AiIncidentResponseAgent.Application.Models;
using AiIncidentResponseAgent.Domain.Executions;

namespace AiIncidentResponseAgent.Application.Actions;

public interface IAgentActionHandler
{
    AgentAction Action { get; }

    Task<AgentActionResult> HandleAsync(
        AgentContext context,
        CancellationToken cancellationToken = default);
}
