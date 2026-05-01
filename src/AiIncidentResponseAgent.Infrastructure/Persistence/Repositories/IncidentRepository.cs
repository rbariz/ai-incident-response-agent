using AiIncidentResponseAgent.Application.Abstractions.Repositories;
using AiIncidentResponseAgent.Domain.Incidents;

using Microsoft.EntityFrameworkCore;

namespace AiIncidentResponseAgent.Infrastructure.Persistence.Repositories;

public sealed class IncidentRepository : IIncidentRepository
{
    private readonly AgentDbContext _db;

    public IncidentRepository(AgentDbContext db)
    {
        _db = db;
    }

    public Task<Incident?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _db.Incidents.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task AddAsync(Incident incident, CancellationToken cancellationToken = default)
    {
        await _db.Incidents.AddAsync(incident, cancellationToken);
    }

    public async Task<IReadOnlyList<Incident>> GetLatestAsync(
    int take,
    CancellationToken cancellationToken = default)
    {
        return await _db.Incidents
            .OrderByDescending(x => x.CreatedAtUtc)
            .Take(take)
            .ToListAsync(cancellationToken);
    }
    public async Task<IReadOnlyList<Incident>> GetLatestByStatusAsync(
    string? status,
    int take,
    CancellationToken cancellationToken = default)
    {
        var query = _db.Incidents.AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) &&
            Enum.TryParse<IncidentStatus>(status, true, out var parsedStatus))
        {
            query = query.Where(x => x.Status == parsedStatus);
        }

        return await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .Take(take)
            .ToListAsync(cancellationToken);
    }
}
