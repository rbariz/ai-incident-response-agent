namespace AiIncidentResponseAgent.Application.Models
{
    public sealed class PolicyCheckResult
    {
        public bool Allowed { get; init; }

        public string Reason { get; init; } = string.Empty;

        public static PolicyCheckResult Allow() => new()
        {
            Allowed = true,
            Reason = "Allowed"
        };

        public static PolicyCheckResult Deny(string reason) => new()
        {
            Allowed = false,
            Reason = reason
        };
    }


}
