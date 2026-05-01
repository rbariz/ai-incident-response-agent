using AiIncidentResponseAgent.Domain.Executions;

namespace AiIncidentResponseAgent.Application.Abstractions.Repositories
{
    public interface IAgentExecutionRepository
    {
        Task<AgentExecution?> GetByIdempotencyKeyAsync(
            string idempotencyKey,
            CancellationToken cancellationToken = default);

        Task AddAsync(AgentExecution execution, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<AgentExecution>> GetLatestAsync(
    int take,
    CancellationToken cancellationToken = default);

        Task<IReadOnlyList<AgentExecution>> GetLatestByCorrelationAsync(
    string? correlationId,
    int take,
    CancellationToken cancellationToken = default);
    }
}
