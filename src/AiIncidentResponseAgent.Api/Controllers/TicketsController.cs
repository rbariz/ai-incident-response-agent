using AiIncidentResponseAgent.Application.Abstractions;
using AiIncidentResponseAgent.Application.Abstractions.Repositories;
using AiIncidentResponseAgent.Contracts.Common;
using AiIncidentResponseAgent.Contracts.Ops;
using AiIncidentResponseAgent.Contracts.Tickets;
using AiIncidentResponseAgent.Domain.Ticketing;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiIncidentResponseAgent.Api.Controllers;

[Authorize(Policy = "CanViewOps")]
[ApiController]
[Route("api/tickets")]
public sealed class TicketsController : ControllerBase
{
    private readonly ITicketRepository _tickets;
    private readonly IUnitOfWork _unitOfWork;

    public TicketsController(
        ITicketRepository tickets,
        IUnitOfWork unitOfWork)
    {
        _tickets = tickets;
        _unitOfWork = unitOfWork;
    }

    [HttpPost]
    [Authorize(Policy = "CanManageTickets")]
    public async Task<ActionResult<TicketResponse>> Create(
        [FromBody] CreateTicketRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.TicketCode))
        {
            return BadRequest("TicketCode is required.");
        }

        var existing = await _tickets.GetByCodeAsync(
            request.TicketCode.Trim(),
            cancellationToken);

        if (existing is not null)
        {
            return Conflict($"Ticket '{request.TicketCode}' already exists.");
        }

        var ticket = new Ticket(request.TicketCode.Trim());

        await _tickets.AddAsync(ticket, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(
            nameof(GetLatest),
            new { },
            ToResponse(ticket));
    }

    
    [HttpGet]
    public async Task<ActionResult<PagedResponse<TicketResponse>>> GetLatest(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 50,
    CancellationToken cancellationToken = default)
    {
        var tickets = await _tickets.GetPagedAsync(
            page,
            pageSize,
            cancellationToken);

        return Ok(new PagedResponse<TicketResponse>
        {
            Items = tickets.Items.Select(ToResponse).ToList(),
            Page = tickets.Page,
            PageSize = tickets.PageSize,
            TotalCount = tickets.TotalCount
        });
    }

    private static TicketResponse ToResponse(Ticket ticket) => new()
    {
        Id = ticket.Id,
        TicketCode = ticket.TicketCode,
        Status = ticket.Status.ToString(),
        BlockedReason = ticket.BlockedReason,
        BlockedAtUtc = ticket.BlockedAtUtc,
        CreatedAtUtc = ticket.CreatedAtUtc
    };
}