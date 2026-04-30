using AiIncidentResponseAgent.Domain.Actions;
using AiIncidentResponseAgent.Domain.Executions;

namespace AiIncidentResponseAgent.Application.Abstractions.Repositories
{
    public interface IAgentActionLockRepository
    {
        Task<bool> ExistsAsync(
            AgentAction action,
            string correlationId,
            CancellationToken cancellationToken = default);

        Task AddAsync(
            AgentActionLock actionLock,
            CancellationToken cancellationToken = default);
    }
}
