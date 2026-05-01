using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiIncidentResponseAgent.Contracts.Ops;

public sealed class AgentExecutionResponse
{
    public Guid Id { get; set; }
    public Guid AgentEventId { get; set; }
    public Guid? IncidentId { get; set; }

    public string CorrelationId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Decision { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;

    public string AnalysisProvider { get; set; } = string.Empty;
    public string AnalysisSummary { get; set; } = string.Empty;
    public decimal ConfidenceScore { get; set; }

    public string ResultJson { get; set; } = "{}";
    public string ErrorMessage { get; set; } = string.Empty;

    public int RetryCount { get; set; }

    public DateTime CreatedAtUtc { get; set; }
    public DateTime? StartedAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }

    public string AnalysisLanguage { get; set; } = "en";
    public string AnalysisSummaryFr { get; set; } = string.Empty;
    public string AnalysisSummaryEn { get; set; } = string.Empty;

}

