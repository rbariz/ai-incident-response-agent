using AiIncidentResponseAgent.Domain.Incidents;

namespace AiIncidentResponseAgent.Application.Abstractions.Repositories
{
    public interface IIncidentRepository
    {
        Task<Incident?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        Task AddAsync(Incident incident, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<Incident>> GetLatestAsync(
    int take,
    CancellationToken cancellationToken = default);
    }
}
