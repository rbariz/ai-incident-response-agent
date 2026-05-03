using AiIncidentResponseAgent.Application.Actions;
using AiIncidentResponseAgent.Application.Models;
using AiIncidentResponseAgent.Domain.Executions;

namespace AiIncidentResponseAgent.Infrastructure.Actions.Handlers;

public sealed class CreateIncidentActionHandler : IAgentActionHandler
{
    public AgentAction Action => AgentAction.CreateIncident;

    public Task<AgentActionResult> HandleAsync(
        AgentContext context,
        CancellationToken cancellationToken = default)
    {
        var result = $$"""
        {
          "action": "CreateIncident",
          "status": "CreatedByOrchestrator",
          "eventId": "{{context.Event.Id}}",
          "executedAtUtc": "{{DateTime.UtcNow:O}}"
        }
        """;

        return Task.FromResult(AgentActionResult.Ok(result));
    }
}
