using AiIncidentResponseAgent.Application.Abstractions.Repositories;
using AiIncidentResponseAgent.Contracts.Metrics;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiIncidentResponseAgent.Api.Controllers;

[ApiController]
[Route("api/metrics")]
[Authorize(Policy = "CanViewOps")]
public sealed class MetricsController : ControllerBase
{
    private readonly IAgentMetricsRepository _metrics;

    public MetricsController(IAgentMetricsRepository metrics)
    {
        _metrics = metrics;
    }

    [HttpGet("overview")]
    public async Task<ActionResult<AgentMetricsResponse>> GetOverview(
        CancellationToken cancellationToken)
    {
        var result = await _metrics.GetOverviewAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("technical")]
    public async Task<ActionResult<AgentTechnicalMetricsResponse>> GetTechnical(
    CancellationToken cancellationToken)
    {
        var result = await _metrics.GetTechnicalAsync(cancellationToken);
        return Ok(result);
    }
}