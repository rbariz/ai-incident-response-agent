using AiIncidentResponseAgent.Domain.Common;

namespace AiIncidentResponseAgent.Domain.Events
{
    public sealed class AgentEvent : Entity
    {
        private AgentEvent() { }

        public AgentEvent(
            AgentEventType type,
            string source,
            string payloadJson,
            string correlationId)
        {
            Type = type;
            Source = source;
            PayloadJson = payloadJson;
            CorrelationId = correlationId;
        }

        public AgentEventType Type { get; private set; }
        public string Source { get; private set; } = string.Empty;
        public string PayloadJson { get; private set; } = "{}";
        public string CorrelationId { get; private set; } = string.Empty;

        public bool Processed { get; private set; }
        public DateTime? ProcessedAtUtc { get; private set; }

        public void MarkProcessed()
        {
            Processed = true;
            ProcessedAtUtc = DateTime.UtcNow;
        }
    }
}
