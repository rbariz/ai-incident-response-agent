using AiIncidentResponseAgent.Application.Models;

namespace AiIncidentResponseAgent.Application.Abstractions
{
    public interface IAgentMemoryService
    {
        Task<string> LoadMemoryAsync(
            AgentContext context,
            CancellationToken cancellationToken = default);

        Task UpdateMemoryAsync(
            AgentContext context,
            AgentActionResult actionResult,
            CancellationToken cancellationToken = default);
    }
}
