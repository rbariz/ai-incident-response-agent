using AiIncidentResponseAgent.Application.Abstractions.Repositories;

using Microsoft.AspNetCore.Mvc;

namespace AiIncidentResponseAgent.Api.Controllers;

[ApiController]
[Route("api/agent-executions")]
public sealed class AgentExecutionsController : ControllerBase
{
    private readonly IAgentExecutionRepository _executions;

    public AgentExecutionsController(IAgentExecutionRepository executions)
    {
        _executions = executions;
    }

    [HttpGet]
    public async Task<IActionResult> GetLatest(
        [FromQuery] int take = 50,
        CancellationToken cancellationToken = default)
    {
        take = Math.Clamp(take, 1, 200);

        var executions = await _executions.GetLatestAsync(take, cancellationToken);

        return Ok(executions.Select(x => new
        {
            x.Id,
            x.AgentEventId,
            x.IncidentId,
            x.IdempotencyKey,
            x.CorrelationId,
            x.Status,
            x.Decision,
            x.Action,
            x.AnalysisSummary,
            x.ConfidenceScore,
            x.ResultJson,
            x.ErrorMessage,
            x.RetryCount,
            x.CreatedAtUtc,
            x.StartedAtUtc,
            x.CompletedAtUtc
        }));
    }
}
