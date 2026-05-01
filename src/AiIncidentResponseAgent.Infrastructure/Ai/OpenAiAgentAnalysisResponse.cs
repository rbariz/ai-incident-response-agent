using System.Text.Json.Serialization;

namespace AiIncidentResponseAgent.Infrastructure.Ai;

public sealed partial class StubAgentAnalyzer
{
    public sealed class OpenAiAgentAnalysisResponse
    {
        [JsonPropertyName("summary")]
        public string Summary { get; set; } = string.Empty;

        [JsonPropertyName("intent")]
        public string Intent { get; set; } = string.Empty;

        [JsonPropertyName("confidenceScore")]
        public decimal ConfidenceScore { get; set; }

        [JsonPropertyName("suggestedAction")]
        public string SuggestedAction { get; set; } = string.Empty;
    }
}
