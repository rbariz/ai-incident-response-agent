using static AiIncidentResponseAgent.Infrastructure.Ai.StubAgentAnalyzer;

namespace AiIncidentResponseAgent.Infrastructure.Ai;

public sealed class OpenAiAnalyzerOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "gpt-4.1-mini";
    public bool Enabled { get; set; } = false;
}
