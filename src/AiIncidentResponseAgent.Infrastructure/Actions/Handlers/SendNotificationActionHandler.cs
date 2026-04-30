using AiIncidentResponseAgent.Application.Models;
using AiIncidentResponseAgent.Domain.Executions;

namespace AiIncidentResponseAgent.Infrastructure.Actions.Handlers;

public sealed class SendNotificationActionHandler : IAgentActionHandler
{
    public AgentAction Action => AgentAction.SendNotification;

    public Task<AgentActionResult> HandleAsync(
        AgentContext context,
        CancellationToken cancellationToken = default)
    {
        var result = $$"""
        {
          "action": "SendNotification",
          "status": "Simulated",
          "message": "Agent notification emitted",
          "correlationId": "{{context.Event.CorrelationId}}"
        }
        """;

        return Task.FromResult(AgentActionResult.Ok(result));
    }
}