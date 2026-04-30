using AiIncidentResponseAgent.Application.Abstractions;
using AiIncidentResponseAgent.Application.Models;
using AiIncidentResponseAgent.Domain.Executions;

namespace AiIncidentResponseAgent.Infrastructure.Actions;

public sealed class AgentActionExecutor : IAgentActionExecutor
{
    private readonly IEnumerable<IAgentActionHandler> _handlers;

    public AgentActionExecutor(IEnumerable<IAgentActionHandler> handlers)
    {
        _handlers = handlers;
    }

    public async Task<AgentActionResult> ExecuteAsync(
        AgentAction action,
        AgentContext context,
        CancellationToken cancellationToken = default)
    {
        var handler = _handlers.FirstOrDefault(x => x.Action == action);

        if (handler is null)
        {
            return AgentActionResult.Fail($"No handler registered for action {action}.");
        }

        return await handler.HandleAsync(context, cancellationToken);
    }
}