using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiIncidentResponseAgent.Domain.Executions
{
    public enum AgentExecutionStatus
    {
        Pending = 1,
        Running = 2,
        Succeeded = 3,
        Failed = 4,
        Skipped = 5,

        PendingApproval = 6,
        Approved = 7,
        Rejected = 8,
        RetryScheduled = 9
    }
}
