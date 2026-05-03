using AiIncidentResponseAgent.Domain.Common;

namespace AiIncidentResponseAgent.Domain.Executions
{
    public sealed class AgentExecution : Entity
    {
        private AgentExecution() { }

        public AgentExecution(
            Guid agentEventId,
            string idempotencyKey,
            string correlationId)
        {
            AgentEventId = agentEventId;
            IdempotencyKey = idempotencyKey;
            CorrelationId = correlationId;
            Status = AgentExecutionStatus.Pending;
        }

        public Guid AgentEventId { get; private set; }
        public Guid? IncidentId { get; private set; }

        public string IdempotencyKey { get; private set; } = string.Empty;
        public string CorrelationId { get; private set; } = string.Empty;

        public AgentExecutionStatus Status { get; private set; }
        public AgentDecision Decision { get; private set; }
        public AgentAction Action { get; private set; }

        public string AnalysisSummary { get; private set; } = string.Empty;
        public decimal ConfidenceScore { get; private set; }

        public string ResultJson { get; private set; } = "{}";
        public string ErrorMessage { get; private set; } = string.Empty;

        public int RetryCount { get; private set; }

        public DateTime? StartedAtUtc { get; private set; }
        public DateTime? CompletedAtUtc { get; private set; }

        public string AnalysisProvider { get; private set; } = string.Empty;

        public string AnalysisLanguage { get; private set; } = "en";

        public string AnalysisSummaryFr { get; private set; } = string.Empty;
        public string AnalysisSummaryEn { get; private set; } = string.Empty;

        public string ApprovalReason { get; private set; } = string.Empty;
        public DateTime? ApprovedAtUtc { get; private set; }
        public DateTime? RejectedAtUtc { get; private set; }

        public DateTime? NextRetryAtUtc { get; private set; }
        public DateTime? LastRetryAtUtc { get; private set; }

        public void AttachIncident(Guid incidentId)
        {
            IncidentId = incidentId;
        }

        public void Start()
        {
            Status = AgentExecutionStatus.Running;
            StartedAtUtc = DateTime.UtcNow;
        }

        public void SetDecision(
    AgentDecision decision,
    AgentAction action,
    string analysisSummary,
    decimal confidenceScore,
    string analysisProvider,
    string analysisLanguage,
    string analysisSummaryFr,
    string analysisSummaryEn)
        {
            Decision = decision;
            Action = action;
            AnalysisSummary = analysisSummary;
            ConfidenceScore = confidenceScore;
            AnalysisProvider = analysisProvider;
            AnalysisLanguage = NormalizeLang(analysisLanguage);
            AnalysisSummaryFr = analysisSummaryFr;
            AnalysisSummaryEn = analysisSummaryEn;
        }

        public void MarkSucceeded(string resultJson)
        {
            Status = AgentExecutionStatus.Succeeded;
            ResultJson = resultJson;
            CompletedAtUtc = DateTime.UtcNow;
        }

        public void MarkFailed(string errorMessage)
        {
            Status = AgentExecutionStatus.Failed;
            ErrorMessage = errorMessage;
            CompletedAtUtc = DateTime.UtcNow;
        }

        public void MarkSkipped(string reason)
        {
            Status = AgentExecutionStatus.Skipped;
            ErrorMessage = reason;
            CompletedAtUtc = DateTime.UtcNow;
        }

        public void IncrementRetry()
        {
            RetryCount++;
        }

        public void MarkPendingApproval(string reason)
        {
            Status = AgentExecutionStatus.PendingApproval;
            ApprovalReason = reason;
            CompletedAtUtc = DateTime.UtcNow;
        }

        public void Approve(string reason)
        {
            if (Status != AgentExecutionStatus.PendingApproval)
            {
                throw new InvalidOperationException("Only pending approval executions can be approved.");
            }

            Status = AgentExecutionStatus.Approved;
            ApprovalReason = reason;
            ApprovedAtUtc = DateTime.UtcNow;
        }

        public void Reject(string reason)
        {
            if (Status != AgentExecutionStatus.PendingApproval)
            {
                throw new InvalidOperationException("Only pending approval executions can be rejected.");
            }

            Status = AgentExecutionStatus.Rejected;
            ApprovalReason = reason;
            RejectedAtUtc = DateTime.UtcNow;
            CompletedAtUtc = DateTime.UtcNow;
        }

        public void MarkApprovedAndRunning(string reason)
        {
            if (Status != AgentExecutionStatus.PendingApproval)
            {
                throw new InvalidOperationException("Only pending approval executions can be approved.");
            }

            Status = AgentExecutionStatus.Running;
            ApprovalReason = reason;
            ApprovedAtUtc = DateTime.UtcNow;
            StartedAtUtc ??= DateTime.UtcNow;
        }


        public void ScheduleRetry(string errorMessage, DateTime nextRetryAtUtc)
        {
            Status = AgentExecutionStatus.RetryScheduled;
            ErrorMessage = errorMessage;
            RetryCount++;
            NextRetryAtUtc = nextRetryAtUtc;
            CompletedAtUtc = DateTime.UtcNow;
        }

        public void StartRetry()
        {
            if (Status != AgentExecutionStatus.RetryScheduled)
            {
                throw new InvalidOperationException("Only retry scheduled executions can be retried.");
            }

            Status = AgentExecutionStatus.Running;
            LastRetryAtUtc = DateTime.UtcNow;
            NextRetryAtUtc = null;
        }

        public void MarkFinalFailed(string errorMessage)
        {
            Status = AgentExecutionStatus.Failed;
            ErrorMessage = errorMessage;
            CompletedAtUtc = DateTime.UtcNow;
            NextRetryAtUtc = null;
        }

        private static string NormalizeLang(string? lang)
        {
            return string.Equals(lang, "fr", StringComparison.OrdinalIgnoreCase)
                ? "fr"
                : "en";
        }
    }
}
