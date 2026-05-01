namespace AiIncidentResponseAgent.Contracts.Ops;

public sealed class AgentTimelineItemResponse
{
    public DateTime OccurredAtUtc { get; set; }

    public string ItemType { get; set; } = string.Empty;

    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string CorrelationId { get; set; } = string.Empty;
}

