using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiIncidentResponseAgent.Contracts.Metrics;



public sealed class AgentMetricsResponse
{
    public int TotalEvents { get; set; }
    public int PendingEvents { get; set; }
    public int ProcessedEvents { get; set; }

    public int TotalExecutions { get; set; }
    public int SucceededExecutions { get; set; }
    public int FailedExecutions { get; set; }
    public int SkippedExecutions { get; set; }
    public int PendingApprovalExecutions { get; set; }

    public int TotalIncidents { get; set; }
    public int OpenIncidents { get; set; }
    public int ResolvedIncidents { get; set; }

    public int TotalTickets { get; set; }
    public int ActiveTickets { get; set; }
    public int BlockedTickets { get; set; }

    public decimal SuccessRate { get; set; }
    public decimal FailureRate { get; set; }
}
