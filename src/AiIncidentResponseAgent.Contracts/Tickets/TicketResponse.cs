namespace AiIncidentResponseAgent.Contracts.Tickets;

public sealed class TicketResponse
{
    public Guid Id { get; set; }
    public string TicketCode { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string BlockedReason { get; set; } = string.Empty;
    public DateTime? BlockedAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
