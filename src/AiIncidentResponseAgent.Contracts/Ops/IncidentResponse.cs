namespace AiIncidentResponseAgent.Contracts.Ops;

public sealed class IncidentResponse
{
    public Guid Id { get; set; }
    public Guid AgentEventId { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public string Severity { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }
    public DateTime? ResolvedAtUtc { get; set; }
}

