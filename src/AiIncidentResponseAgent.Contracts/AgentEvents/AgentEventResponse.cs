namespace AiIncidentResponseAgent.Contracts.AgentEvents
{
    public sealed class AgentEventResponse
    {
        public Guid Id { get; set; }

        //public AgentEventType Type { get; set; }

        public int Type { get; set; }
        public string TypeName { get; set; } = string.Empty;

        public string Source { get; set; } = string.Empty;

        public string PayloadJson { get; set; } = "{}";

        public string CorrelationId { get; set; } = string.Empty;

        public bool Processed { get; set; }

        public DateTime CreatedAtUtc { get; set; }

        public DateTime? ProcessedAtUtc { get; set; }

        public string Lang { get; set; } = "en";
    }


}
