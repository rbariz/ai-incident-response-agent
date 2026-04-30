using AiIncidentResponseAgent.Application.Abstractions.Repositories;
using AiIncidentResponseAgent.Domain.Memory;

using Microsoft.EntityFrameworkCore;

namespace AiIncidentResponseAgent.Infrastructure.Persistence.Repositories;

public sealed class AgentMemoryRepository : IAgentMemoryRepository
{
    private readonly AgentDbContext _db;

    public AgentMemoryRepository(AgentDbContext db)
    {
        _db = db;
    }

    public Task<AgentMemory?> GetByEntityAsync(
        string entityType,
        string entityId,
        CancellationToken cancellationToken = default)
    {
        return _db.AgentMemories.FirstOrDefaultAsync(
            x => x.EntityType == entityType && x.EntityId == entityId,
            cancellationToken);
    }

    public async Task AddAsync(AgentMemory memory, CancellationToken cancellationToken = default)
    {
        await _db.AgentMemories.AddAsync(memory, cancellationToken);
    }
}