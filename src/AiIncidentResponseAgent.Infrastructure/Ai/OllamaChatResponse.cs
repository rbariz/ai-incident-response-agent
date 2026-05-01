using System.Text.Json.Serialization;

namespace AiIncidentResponseAgent.Infrastructure.Ai
{
    public sealed class OllamaChatResponse
    {
        [JsonPropertyName("message")]
        public OllamaChatMessage Message { get; set; } = new();
    }
}
