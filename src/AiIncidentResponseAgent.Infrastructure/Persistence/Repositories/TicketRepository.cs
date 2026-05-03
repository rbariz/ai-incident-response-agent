using AiIncidentResponseAgent.Application.Abstractions.Repositories;
using AiIncidentResponseAgent.Contracts.Common;
using AiIncidentResponseAgent.Domain.Executions;
using AiIncidentResponseAgent.Domain.Ticketing;
using AiIncidentResponseAgent.Infrastructure.Persistence.Paging;

using Microsoft.EntityFrameworkCore;

namespace AiIncidentResponseAgent.Infrastructure.Persistence.Repositories;

public sealed class TicketRepository : ITicketRepository
{
    private readonly AgentDbContext _db;

    public TicketRepository(AgentDbContext db)
    {
        _db = db;
    }

    public Task<Ticket?> GetByCodeAsync(
        string ticketCode,
        CancellationToken cancellationToken = default)
    {
        return _db.Tickets.FirstOrDefaultAsync(
            x => x.TicketCode == ticketCode,
            cancellationToken);
    }

    public async Task AddAsync(
        Ticket ticket,
        CancellationToken cancellationToken = default)
    {
        await _db.Tickets.AddAsync(ticket, cancellationToken);
    }

    public async Task<IReadOnlyList<Ticket>> GetLatestAsync(
        int take,
        CancellationToken cancellationToken = default)
    {
        return await _db.Tickets
            .OrderByDescending(x => x.CreatedAtUtc)
            .Take(take)
            .ToListAsync(cancellationToken);
    }


    public async Task<PagedResponse<Ticket>> GetPagedAsync(
    int page,
    int pageSize,
    CancellationToken cancellationToken = default)
    {
        var query = _db.Tickets.AsQueryable();


        return await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToPagedResponseAsync(page, pageSize, cancellationToken);
    }
}
