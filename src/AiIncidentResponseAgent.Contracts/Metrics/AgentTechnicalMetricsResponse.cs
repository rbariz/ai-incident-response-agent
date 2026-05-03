namespace AiIncidentResponseAgent.Contracts.Metrics;

public sealed class AgentTechnicalMetricsResponse
{
    public double AverageExecutionDurationMs { get; set; }
    public double MaxExecutionDurationMs { get; set; }

    public int TotalRetries { get; set; }
    public int RetryScheduledExecutions { get; set; }

    public IReadOnlyList<MetricCountItem> AiProviderUsage { get; set; } = [];
    public IReadOnlyList<MetricCountItem> ActionUsage { get; set; } = [];
    public IReadOnlyList<MetricCountItem> StatusDistribution { get; set; } = [];
}
