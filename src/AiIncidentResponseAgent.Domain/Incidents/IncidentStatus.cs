namespace AiIncidentResponseAgent.Domain.Incidents
{
    public enum IncidentStatus
    {
        New = 1,
        Analyzing = 2,
        ActionPending = 3,
        ActionExecuted = 4,
        Resolved = 5,
        Failed = 6,
        Escalated = 7
    }
}
