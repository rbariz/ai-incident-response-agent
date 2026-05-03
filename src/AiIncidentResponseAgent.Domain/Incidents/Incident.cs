using AiIncidentResponseAgent.Domain.Common;

namespace AiIncidentResponseAgent.Domain.Incidents
{
    public sealed class Incident : Entity
    {
        private Incident() { }

        public Incident(
            Guid agentEventId,
            string title,
            string description,
            IncidentSeverity severity)
        {
            AgentEventId = agentEventId;
            Title = title;
            Description = description;
            Severity = severity;
            Status = IncidentStatus.New;
        }

        public Guid AgentEventId { get; private set; }
        public string Title { get; private set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;
        public IncidentSeverity Severity { get; private set; }
        public IncidentStatus Status { get; private set; }

        public DateTime? ResolvedAtUtc { get; private set; }

        public bool IsArchived { get; private set; }

        public void MarkAnalyzing() => Status = IncidentStatus.Analyzing;

        public void MarkActionPending() => Status = IncidentStatus.ActionPending;

        public void MarkActionExecuted() => Status = IncidentStatus.ActionExecuted;

        public void Resolve()
        {
            Status = IncidentStatus.Resolved;
            ResolvedAtUtc = DateTime.UtcNow;
        }

        public void Fail() => Status = IncidentStatus.Failed;

        public void Escalate() => Status = IncidentStatus.Escalated;


        public void UpdateDetails(
            string title,
            string description,
            IncidentSeverity severity)
        {
            Title = title.Trim();
            Description = description.Trim();
            Severity = severity;
        }

        public void Reopen()
        {
            Status = IncidentStatus.New;
            ResolvedAtUtc = null;
        }

        public void Archive()
        {
            IsArchived = true;
        }
    }
}
