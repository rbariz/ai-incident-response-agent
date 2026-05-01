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
            string correlationId,
            string lang)
        {
            Type = type;
            Source = source;
            PayloadJson = payloadJson;
            CorrelationId = correlationId;
            Lang = NormalizeLang(lang);
        }

        public AgentEventType Type { get; private set; }
        public string Source { get; private set; } = string.Empty;
        public string PayloadJson { get; private set; } = "{}";
        public string CorrelationId { get; private set; } = string.Empty;

        public string Lang { get; private set; } = "en";

        public bool Processed { get; private set; }
        public DateTime? ProcessedAtUtc { get; private set; }

        public void MarkProcessed()
        {
            Processed = true;
            ProcessedAtUtc = DateTime.UtcNow;
        }
        private static string NormalizeLang(string? lang)
        {
            return string.Equals(lang, "fr", StringComparison.OrdinalIgnoreCase)
                ? "fr"
                : "en";
        }
    }
}
