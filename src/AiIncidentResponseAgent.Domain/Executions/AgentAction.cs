namespace AiIncidentResponseAgent.Domain.Executions
{
    public enum AgentAction
    {
        None = 0,
        BlockTicket = 1,
        RestartService = 2,
        SendNotification = 3,
        CreateIncident = 4,
        Escalate = 5,
        Retry = 6
    }
}
