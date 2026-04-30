using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiIncidentResponseAgent.Domain.Events
{
    public enum AgentEventType
    {
        Unknown = 0,
        DuplicateScan = 1,
        FraudRiskDetected = 2,
        ApiErrorSpike = 3,
        SystemMetricAlert = 4,
        SuspiciousBusinessActivity = 5
    }
}
