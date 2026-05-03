using AiIncidentResponseAgent.Application.Models;
using AiIncidentResponseAgent.Contracts.Metrics;

namespace AiIncidentResponseAgent.Application.Abstractions.Repositories
{
    public interface IAgentMetricsRepository
    {
        Task<AgentMetricsResponse> GetOverviewAsync(
            CancellationToken cancellationToken = default);

        Task<AgentTechnicalMetricsResponse> GetTechnicalAsync(
    CancellationToken cancellationToken = default);
    }


    
}
