using AiIncidentResponseAgent.Application.Models;

namespace AiIncidentResponseAgent.Application.Abstractions
{
    public interface IAgentAnalyzer
    {
        Task<AgentAnalysisResult> AnalyzeAsync(
            AgentContext context,
            CancellationToken cancellationToken = default);
    }
}
