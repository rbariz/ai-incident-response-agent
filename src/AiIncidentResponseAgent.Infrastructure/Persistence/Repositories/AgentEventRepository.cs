using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AiIncidentResponseAgent.Application.Abstractions.Repositories;
using AiIncidentResponseAgent.Domain.Events;

using Microsoft.EntityFrameworkCore;

namespace AiIncidentResponseAgent.Infrastructure.Persistence.Repositories;

public sealed class AgentEventRepository : IAgentEventRepository
{
    private readonly AgentDbContext _db;

    public AgentEventRepository(AgentDbContext db)
    {
        _db = db;
    }

    public Task<AgentEvent?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _db.AgentEvents.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<AgentEvent>> GetUnprocessedAsync(
        int take,
        CancellationToken cancellationToken = default)
    {
        return await _db.AgentEvents
            .Where(x => !x.Processed)
            .OrderBy(x => x.CreatedAtUtc)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(AgentEvent agentEvent, CancellationToken cancellationToken = default)
    {
        await _db.AgentEvents.AddAsync(agentEvent, cancellationToken);
    }

    public async Task<IReadOnlyList<AgentEvent>> GetLatestAsync(
    int take,
    CancellationToken cancellationToken = default)
    {
        return await _db.AgentEvents
            .OrderByDescending(x => x.CreatedAtUtc)
            .Take(take)
            .ToListAsync(cancellationToken);
    }
}
