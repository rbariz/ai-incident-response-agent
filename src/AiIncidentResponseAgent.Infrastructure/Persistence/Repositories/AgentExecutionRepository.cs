using AiIncidentResponseAgent.Application.Abstractions.Repositories;
using AiIncidentResponseAgent.Domain.Executions;

using Microsoft.EntityFrameworkCore;

namespace AiIncidentResponseAgent.Infrastructure.Persistence.Repositories;

public sealed class AgentExecutionRepository : IAgentExecutionRepository
{
    private readonly AgentDbContext _db;

    public AgentExecutionRepository(AgentDbContext db)
    {
        _db = db;
    }

    public Task<AgentExecution?> GetByIdempotencyKeyAsync(
        string idempotencyKey,
        CancellationToken cancellationToken = default)
    {
        return _db.AgentExecutions
            .FirstOrDefaultAsync(x => x.IdempotencyKey == idempotencyKey, cancellationToken);
    }

    public async Task AddAsync(AgentExecution execution, CancellationToken cancellationToken = default)
    {
        await _db.AgentExecutions.AddAsync(execution, cancellationToken);
    }

    public async Task<IReadOnlyList<AgentExecution>> GetLatestAsync(
    int take,
    CancellationToken cancellationToken = default)
    {
        return await _db.AgentExecutions
            .OrderByDescending(x => x.CreatedAtUtc)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AgentExecution>> GetLatestByCorrelationAsync(
    string? correlationId,
    int take,
    CancellationToken cancellationToken = default)
    {
        var query = _db.AgentExecutions.AsQueryable();

        if (!string.IsNullOrWhiteSpace(correlationId))
        {
            query = query.Where(x => x.CorrelationId == correlationId);
        }

        return await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .Take(take)
            .ToListAsync(cancellationToken);
    }
}
