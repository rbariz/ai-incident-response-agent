namespace AiIncidentResponseAgent.Application.Models
{
    public sealed class AgentAnalysisResult
    {
        public string Summary { get; init; } = string.Empty;

        public string Intent { get; init; } = string.Empty;

        public decimal ConfidenceScore { get; init; }

        public string SuggestedAction { get; init; } = string.Empty;

        public string RawResponseJson { get; init; } = "{}";
    }


}
