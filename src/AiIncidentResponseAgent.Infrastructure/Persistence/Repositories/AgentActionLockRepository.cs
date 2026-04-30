using AiIncidentResponseAgent.Application.Abstractions.Repositories;
using AiIncidentResponseAgent.Domain.Actions;
using AiIncidentResponseAgent.Domain.Executions;

using Microsoft.EntityFrameworkCore;

namespace AiIncidentResponseAgent.Infrastructure.Persistence.Repositories;

public sealed class AgentActionLockRepository : IAgentActionLockRepository
{
    private readonly AgentDbContext _db;

    public AgentActionLockRepository(AgentDbContext db)
    {
        _db = db;
    }

    public Task<bool> ExistsAsync(
        AgentAction action,
        string correlationId,
        CancellationToken cancellationToken = default)
    {
        return _db.AgentActionLocks.AnyAsync(
            x => x.Action == action && x.CorrelationId == correlationId,
            cancellationToken);
    }

    public async Task AddAsync(
        AgentActionLock actionLock,
        CancellationToken cancellationToken = default)
    {
        await _db.AgentActionLocks.AddAsync(actionLock, cancellationToken);
    }
}
