using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiIncidentResponseAgent.Contracts.Tickets;
public sealed class CreateTicketRequest
{
    public string TicketCode { get; set; } = string.Empty;
}
