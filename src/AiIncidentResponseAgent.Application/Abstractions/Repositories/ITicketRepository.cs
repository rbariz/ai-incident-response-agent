using AiIncidentResponseAgent.Contracts.Common;
using AiIncidentResponseAgent.Domain.Executions;
using AiIncidentResponseAgent.Domain.Ticketing;

namespace AiIncidentResponseAgent.Application.Abstractions.Repositories
{
    public interface ITicketRepository
    {
        Task<Ticket?> GetByCodeAsync(
            string ticketCode,
            CancellationToken cancellationToken = default);

        Task AddAsync(
            Ticket ticket,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<Ticket>> GetLatestAsync(
            int take,
            CancellationToken cancellationToken = default);

        Task<PagedResponse<Ticket>> GetPagedAsync(
    int page,
    int pageSize,
    CancellationToken cancellationToken = default);
    }
}
