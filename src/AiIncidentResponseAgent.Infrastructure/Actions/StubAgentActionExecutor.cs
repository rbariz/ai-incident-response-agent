using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AiIncidentResponseAgent.Application.Abstractions;
using AiIncidentResponseAgent.Application.Models;
using AiIncidentResponseAgent.Domain.Executions;

namespace AiIncidentResponseAgent.Infrastructure.Actions;
public sealed class StubAgentActionExecutor : IAgentActionExecutor
{
    public Task<AgentActionResult> ExecuteAsync(
        AgentAction action,
        AgentContext context,
        CancellationToken cancellationToken = default)
    {
        var resultJson = $$"""
        {
          "provider": "stub",
          "action": "{{action}}",
          "eventId": "{{context.Event.Id}}",
          "correlationId": "{{context.Event.CorrelationId}}",
          "executedAtUtc": "{{DateTime.UtcNow:O}}"
        }
        """;

        return Task.FromResult(AgentActionResult.Ok(resultJson));
    }
}
