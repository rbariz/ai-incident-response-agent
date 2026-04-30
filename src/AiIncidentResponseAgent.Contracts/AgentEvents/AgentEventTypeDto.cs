namespace AiIncidentResponseAgent.Contracts.AgentEvents
{
    public enum AgentEventTypeDto
    {
        Unknown = 0,
        DuplicateScan = 1,
        FraudRiskDetected = 2,
        ApiErrorSpike = 3,
        SystemMetricAlert = 4,
        SuspiciousBusinessActivity = 5
    }


}
