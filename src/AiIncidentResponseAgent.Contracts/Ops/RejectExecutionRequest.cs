namespace AiIncidentResponseAgent.Contracts.Ops;

public sealed class RejectExecutionRequest
{
    public string Reason { get; set; } = string.Empty;
}

