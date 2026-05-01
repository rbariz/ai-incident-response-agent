using AiIncidentResponseAgent.Application.Abstractions.Repositories;
using AiIncidentResponseAgent.Contracts.Ops;
using AiIncidentResponseAgent.Domain.Executions;

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
    public async Task<ActionResult<IReadOnlyList<AgentExecutionResponse>>> GetLatest(
        [FromQuery] string? correlationId,
        [FromQuery] int take = 200,
        CancellationToken cancellationToken = default)
    {
        take = Math.Clamp(take, 1, 200);

        var executions = await _executions.GetLatestByCorrelationAsync(
            correlationId,
            take,
            cancellationToken);

        return Ok(executions.Select(ToResponse).ToList());
    }

    private static AgentExecutionResponse ToResponse(AgentExecution x) => new()
    {
        Id = x.Id,
        AgentEventId = x.AgentEventId,
        IncidentId = x.IncidentId,
        CorrelationId = x.CorrelationId,
        Status = x.Status.ToString(),
        Decision = x.Decision.ToString(),
        Action = x.Action.ToString(),
        AnalysisProvider = x.AnalysisProvider,
        AnalysisSummary = x.AnalysisSummary,
        ConfidenceScore = x.ConfidenceScore,
        ResultJson = x.ResultJson,
        ErrorMessage = x.ErrorMessage,
        RetryCount = x.RetryCount,
        CreatedAtUtc = x.CreatedAtUtc,
        StartedAtUtc = x.StartedAtUtc,
        CompletedAtUtc = x.CompletedAtUtc,
        AnalysisLanguage = x.AnalysisLanguage,
        AnalysisSummaryFr = x.AnalysisSummaryFr,
        AnalysisSummaryEn = x.AnalysisSummaryEn,
    };
}