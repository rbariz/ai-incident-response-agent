namespace AiIncidentResponseAgent.Application.Models
{
    public sealed class RetryOptions
    {
        public int MaxRetries { get; set; } = 3;
        public int BaseDelaySeconds { get; set; } = 10;
        public int MaxDelaySeconds { get; set; } = 300;
    }


}
