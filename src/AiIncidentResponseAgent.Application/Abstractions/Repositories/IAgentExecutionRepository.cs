using AiIncidentResponseAgent.Contracts.Common;
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

        Task<AgentExecution?> GetByIdAsync(
    Guid id,
    CancellationToken cancellationToken = default);


        Task<PagedResponse<AgentExecution>> GetPagedAsync(
    string? correlationId,
    int page,
    int pageSize,
    CancellationToken cancellationToken = default);

        Task<IReadOnlyList<AgentExecution>> GetRetryDueAsync(
    int take,
    DateTime nowUtc,
    CancellationToken cancellationToken = default);
    }
}
