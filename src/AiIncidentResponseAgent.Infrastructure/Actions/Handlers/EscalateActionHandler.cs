using AiIncidentResponseAgent.Application.Actions;
using AiIncidentResponseAgent.Application.Models;
using AiIncidentResponseAgent.Domain.Executions;

namespace AiIncidentResponseAgent.Infrastructure.Actions.Handlers;

public sealed class EscalateActionHandler : IAgentActionHandler
{
    public AgentAction Action => AgentAction.Escalate;

    public Task<AgentActionResult> HandleAsync(
        AgentContext context,
        CancellationToken cancellationToken = default)
    {
        var result = $$"""
        {
          "action": "Escalate",
          "status": "Simulated",
          "message": "Incident escalated to human operator.",
          "eventId": "{{context.Event.Id}}",
          "correlationId": "{{context.Event.CorrelationId}}",
          "executedAtUtc": "{{DateTime.UtcNow:O}}"
        }
        """;

        return Task.FromResult(AgentActionResult.Ok(result));
    }
}
