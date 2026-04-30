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
}
