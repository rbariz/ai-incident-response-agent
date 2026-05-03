using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AiIncidentResponseAgent.Contracts.Common;
using AiIncidentResponseAgent.Domain.Events;
using AiIncidentResponseAgent.Domain.Executions;

namespace AiIncidentResponseAgent.Application.Abstractions.Repositories
{
    public interface IAgentEventRepository
    {
        Task<AgentEvent?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<AgentEvent>> GetUnprocessedAsync(
            int take,
            CancellationToken cancellationToken = default);

        Task AddAsync(AgentEvent agentEvent, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<AgentEvent>> GetLatestAsync(
    int take,
    CancellationToken cancellationToken = default);

        Task<PagedResponse<AgentEvent>> GetPagedAsync(
    int page,
    int pageSize,
    CancellationToken cancellationToken = default);
    }
}
