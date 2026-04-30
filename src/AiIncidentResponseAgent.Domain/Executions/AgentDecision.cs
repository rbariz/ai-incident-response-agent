namespace AiIncidentResponseAgent.Domain.Executions
{
    public enum AgentDecision
    {
        None = 0,
        ObserveOnly = 1,
        SuggestAction = 2,
        ExecuteAction = 3,
        ExecuteAndEscalate = 4,
        RejectAction = 5
    }
}
