using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AiIncidentResponseAgent.Domain.Common;

namespace AiIncidentResponseAgent.Domain.Ticketing;
public sealed class Ticket : Entity
{
    private Ticket() { }

    public Ticket(string ticketCode)
    {
        TicketCode = ticketCode;
        Status = TicketStatus.Active;
    }

    public string TicketCode { get; private set; } = string.Empty;
    public TicketStatus Status { get; private set; }
    public string BlockedReason { get; private set; } = string.Empty;
    public DateTime? BlockedAtUtc { get; private set; }

    public void Block(string reason)
    {
        if (Status == TicketStatus.Blocked)
            return;

        Status = TicketStatus.Blocked;
        BlockedReason = reason;
        BlockedAtUtc = DateTime.UtcNow;
    }
}
