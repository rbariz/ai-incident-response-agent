using AiIncidentResponseAgent.Domain.Memory;

namespace AiIncidentResponseAgent.Application.Abstractions.Repositories
{
    public interface IAgentMemoryRepository
    {
        Task<AgentMemory?> GetByEntityAsync(
            string entityType,
            string entityId,
            CancellationToken cancellationToken = default);

        Task AddAsync(AgentMemory memory, CancellationToken cancellationToken = default);
    }
}
