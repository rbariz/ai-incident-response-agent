using AiIncidentResponseAgent.Contracts.Common;
using AiIncidentResponseAgent.Domain.Executions;
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

        Task<IReadOnlyList<Incident>> GetLatestByStatusAsync(
    string? status,
    int take,
    CancellationToken cancellationToken = default);

        Task<PagedResponse<Incident>> GetPagedAsync(
    int page,
    int pageSize,
    CancellationToken cancellationToken = default);

    }
}
