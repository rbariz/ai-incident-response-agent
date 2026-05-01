namespace AiIncidentResponseAgent.Infrastructure.Ai;

public sealed class OllamaAnalyzerOptions
{
    public bool Enabled { get; set; } = true;
    public string BaseUrl { get; set; } = "http://localhost:11434";
    public string Model { get; set; } = "llama3.2";
}
