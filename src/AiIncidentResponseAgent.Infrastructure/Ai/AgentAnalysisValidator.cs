using AiIncidentResponseAgent.Application.Models;
using AiIncidentResponseAgent.Domain.Executions;

using static AiIncidentResponseAgent.Infrastructure.Ai.StubAgentAnalyzer;

namespace AiIncidentResponseAgent.Infrastructure.Ai;

public static class AgentAnalysisValidator
{
    public static bool IsValid(OpenAiAgentAnalysisResponse? response)
    {
        if (response is null) return false;
        if (string.IsNullOrWhiteSpace(response.Summary)) return false;
        if (string.IsNullOrWhiteSpace(response.Intent)) return false;
        if (response.ConfidenceScore < 0m || response.ConfidenceScore > 1m) return false;

        return Enum.TryParse<AgentAction>(
            response.SuggestedAction,
            ignoreCase: true,
            out _);
    }

    public static AgentAnalysisResult ToResult(
        OpenAiAgentAnalysisResponse response,
        string rawJson,
        string provider)
    {
        return new AgentAnalysisResult
        {
            Summary = response.Summary.Trim(),
            Intent = response.Intent.Trim(),
            ConfidenceScore = Math.Clamp(response.ConfidenceScore, 0m, 1m),
            SuggestedAction = NormalizeAction(response.SuggestedAction),
            RawResponseJson = rawJson,
            Provider = provider
        };
    }

    private static string NormalizeAction(string value)
    {
        return Enum.TryParse<AgentAction>(value, true, out var action)
            ? action.ToString()
            : AgentAction.None.ToString();
    }
}
