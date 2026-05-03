using AiIncidentResponseAgent.Application.Abstractions.Repositories;
using AiIncidentResponseAgent.Contracts.Metrics;
using AiIncidentResponseAgent.Domain.Executions;
using AiIncidentResponseAgent.Domain.Incidents;
using AiIncidentResponseAgent.Domain.Ticketing;

using Microsoft.EntityFrameworkCore;

namespace AiIncidentResponseAgent.Infrastructure.Persistence.Repositories;

public sealed class AgentMetricsRepository : IAgentMetricsRepository
{
    private readonly AgentDbContext _db;

    public AgentMetricsRepository(AgentDbContext db)
    {
        _db = db;
    }

    public async Task<AgentMetricsResponse> GetOverviewAsync(
        CancellationToken cancellationToken = default)
    {
        var totalEvents = await _db.AgentEvents.CountAsync(cancellationToken);
        var pendingEvents = await _db.AgentEvents.CountAsync(x => !x.Processed, cancellationToken);
        var processedEvents = await _db.AgentEvents.CountAsync(x => x.Processed, cancellationToken);

        var totalExecutions = await _db.AgentExecutions.CountAsync(cancellationToken);
        var succeededExecutions = await _db.AgentExecutions.CountAsync(
            x => x.Status == AgentExecutionStatus.Succeeded,
            cancellationToken);

        var failedExecutions = await _db.AgentExecutions.CountAsync(
            x => x.Status == AgentExecutionStatus.Failed,
            cancellationToken);

        var skippedExecutions = await _db.AgentExecutions.CountAsync(
            x => x.Status == AgentExecutionStatus.Skipped,
            cancellationToken);

        var pendingApprovalExecutions = await _db.AgentExecutions.CountAsync(
            x => x.Status == AgentExecutionStatus.PendingApproval,
            cancellationToken);

        var totalIncidents = await _db.Incidents.CountAsync(cancellationToken);

        var resolvedIncidents = await _db.Incidents.CountAsync(
            x => x.Status == IncidentStatus.Resolved,
            cancellationToken);

        var openIncidents = await _db.Incidents.CountAsync(
            x => x.Status != IncidentStatus.Resolved,
            cancellationToken);

        var totalTickets = await _db.Tickets.CountAsync(cancellationToken);

        var activeTickets = await _db.Tickets.CountAsync(
            x => x.Status == TicketStatus.Active,
            cancellationToken);

        var blockedTickets = await _db.Tickets.CountAsync(
            x => x.Status == TicketStatus.Blocked,
            cancellationToken);

        var successRate = totalExecutions == 0
            ? 0
            : Math.Round((decimal)succeededExecutions / totalExecutions * 100, 2);

        var failureRate = totalExecutions == 0
            ? 0
            : Math.Round((decimal)failedExecutions / totalExecutions * 100, 2);

        return new AgentMetricsResponse
        {
            TotalEvents = totalEvents,
            PendingEvents = pendingEvents,
            ProcessedEvents = processedEvents,

            TotalExecutions = totalExecutions,
            SucceededExecutions = succeededExecutions,
            FailedExecutions = failedExecutions,
            SkippedExecutions = skippedExecutions,
            PendingApprovalExecutions = pendingApprovalExecutions,

            TotalIncidents = totalIncidents,
            OpenIncidents = openIncidents,
            ResolvedIncidents = resolvedIncidents,

            TotalTickets = totalTickets,
            ActiveTickets = activeTickets,
            BlockedTickets = blockedTickets,

            SuccessRate = successRate,
            FailureRate = failureRate
        };
    }

    public async Task<AgentTechnicalMetricsResponse> GetTechnicalAsync(
    CancellationToken cancellationToken = default)
    {
        var completedExecutions = _db.AgentExecutions
            .Where(x => x.StartedAtUtc != null && x.CompletedAtUtc != null);

        var durations = await completedExecutions
            .Select(x => new
            {
                DurationMs = (x.CompletedAtUtc!.Value - x.StartedAtUtc!.Value).TotalMilliseconds
            })
            .ToListAsync(cancellationToken);

        var aiProviderUsage = await _db.AgentExecutions
            .Where(x => x.AnalysisProvider != "")
            .GroupBy(x => x.AnalysisProvider)
            .Select(g => new MetricCountItem
            {
                Name = g.Key,
                Count = g.Count()
            })
            .ToListAsync(cancellationToken);

        var actionUsageRaw = await _db.AgentExecutions
            .GroupBy(x => x.Action)
            .Select(g => new
            {
                Action = g.Key,
                Count = g.Count()
            })
            .ToListAsync(cancellationToken);

        var statusDistributionRaw = await _db.AgentExecutions
            .GroupBy(x => x.Status)
            .Select(g => new
            {
                Status = g.Key,
                Count = g.Count()
            })
            .ToListAsync(cancellationToken);

        return new AgentTechnicalMetricsResponse
        {
            AverageExecutionDurationMs = durations.Count == 0
                ? 0
                : Math.Round(durations.Average(x => x.DurationMs), 2),

            MaxExecutionDurationMs = durations.Count == 0
                ? 0
                : Math.Round(durations.Max(x => x.DurationMs), 2),

            TotalRetries = await _db.AgentExecutions.SumAsync(
                x => x.RetryCount,
                cancellationToken),

            RetryScheduledExecutions = await _db.AgentExecutions.CountAsync(
                x => x.Status == AgentExecutionStatus.RetryScheduled,
                cancellationToken),

            AiProviderUsage = aiProviderUsage,

            ActionUsage = actionUsageRaw
                .Select(x => new MetricCountItem
                {
                    Name = x.Action.ToString(),
                    Count = x.Count
                })
                .ToList(),

            StatusDistribution = statusDistributionRaw
                .Select(x => new MetricCountItem
                {
                    Name = x.Status.ToString(),
                    Count = x.Count
                })
                .ToList()
        };
    }
}
