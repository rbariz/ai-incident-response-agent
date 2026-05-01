using System.Text.Json.Serialization;

namespace AiIncidentResponseAgent.Infrastructure.Ai
{
    public sealed class OllamaChatMessage
    {
        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;
    }
}
