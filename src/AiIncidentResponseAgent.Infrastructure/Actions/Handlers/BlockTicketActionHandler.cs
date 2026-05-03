using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using AiIncidentResponseAgent.Application.Abstractions.Repositories;
using AiIncidentResponseAgent.Application.Actions;
using AiIncidentResponseAgent.Application.Models;
using AiIncidentResponseAgent.Domain.Executions;

using Microsoft.Extensions.Logging;

namespace AiIncidentResponseAgent.Infrastructure.Actions.Handlers;

public sealed class BlockTicketActionHandler : IAgentActionHandler
{
    private readonly ITicketRepository _tickets;
    private readonly ILogger<BlockTicketActionHandler> _logger;

    public BlockTicketActionHandler(ITicketRepository tickets, ILogger<BlockTicketActionHandler> logger)
    {
        _tickets = tickets;
        _logger = logger;
    }

    public AgentAction Action => AgentAction.BlockTicket;

    public async Task<AgentActionResult> HandleAsync(
        AgentContext context,
        CancellationToken cancellationToken = default)
    {
        var ticketCode = ExtractTicketCode(context.Event.PayloadJson);

        _logger.LogInformation(
    "BlockTicket action requested. TicketCode={TicketCode}, EventId={EventId}, CorrelationId={CorrelationId}",
    ticketCode,
    context.Event.Id,
    context.Event.CorrelationId);

        if (string.IsNullOrWhiteSpace(ticketCode))
        {
            return AgentActionResult.Fail("ticketId was not found in event payload.");
        }

        var ticket = await _tickets.GetByCodeAsync(ticketCode, cancellationToken);

        if (ticket is null)
        {
            return AgentActionResult.Fail($"Ticket '{ticketCode}' was not found.");
        }

        ticket.Block($"Blocked by AI Incident Response Agent. EventId={context.Event.Id}");
        _logger.LogInformation(
    "Ticket blocked successfully. TicketCode={TicketCode}, EventId={EventId}, CorrelationId={CorrelationId}",
    ticket.TicketCode,
    context.Event.Id,
    context.Event.CorrelationId);

        var result = $$"""
        {
          "action": "BlockTicket",
          "status": "Executed",
          "provider": "LocalTicketingModule",
          "ticketCode": "{{ticket.TicketCode}}",
          "ticketStatus": "{{ticket.Status}}",
          "eventId": "{{context.Event.Id}}",
          "correlationId": "{{context.Event.CorrelationId}}",
          "executedAtUtc": "{{DateTime.UtcNow:O}}"
        }
        """;

        return AgentActionResult.Ok(result);
    }

    private static string ExtractTicketCode(string payloadJson)
    {
        using var document = JsonDocument.Parse(payloadJson);

        if (document.RootElement.TryGetProperty("ticketId", out var ticketId))
        {
            return ticketId.GetString() ?? string.Empty;
        }

        if (document.RootElement.TryGetProperty("ticketCode", out var ticketCode))
        {
            return ticketCode.GetString() ?? string.Empty;
        }

        return string.Empty;
    }
}
