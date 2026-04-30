using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AiIncidentResponseAgent.Application.Models;
using AiIncidentResponseAgent.Domain.Executions;

namespace AiIncidentResponseAgent.Infrastructure.Actions.Handlers;

public sealed class BlockTicketActionHandler : IAgentActionHandler
{
    public AgentAction Action => AgentAction.BlockTicket;

    public Task<AgentActionResult> HandleAsync(
        AgentContext context,
        CancellationToken cancellationToken = default)
    {
        var result = $$"""
        {
          "action": "BlockTicket",
          "status": "Simulated",
          "eventId": "{{context.Event.Id}}",
          "correlationId": "{{context.Event.CorrelationId}}",
          "executedAtUtc": "{{DateTime.UtcNow:O}}"
        }
        """;

        return Task.FromResult(AgentActionResult.Ok(result));
    }
}
