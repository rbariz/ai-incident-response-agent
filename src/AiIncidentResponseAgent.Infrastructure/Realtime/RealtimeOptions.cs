namespace AiIncidentResponseAgent.Infrastructure.Realtime;

public sealed class RealtimeOptions
{
    public bool Enabled { get; set; } = true;

    public string ApiBaseUrl { get; set; } = "http://localhost:5027";
}
