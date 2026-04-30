namespace AiIncidentResponseAgent.Application.Models
{
    public sealed class AgentActionResult
    {
        public bool Success { get; init; }

        public string ResultJson { get; init; } = "{}";

        public string ErrorMessage { get; init; } = string.Empty;

        public static AgentActionResult Ok(string resultJson = "{}") => new()
        {
            Success = true,
            ResultJson = resultJson
        };

        public static AgentActionResult Fail(string errorMessage) => new()
        {
            Success = false,
            ErrorMessage = errorMessage
        };
    }


}
