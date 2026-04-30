using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiIncidentResponseAgent.Contracts.AgentEvents
{
    public sealed class CreateAgentEventRequest
    {
        public AgentEventTypeDto Type { get; set; }

        public string Source { get; set; } = string.Empty;

        public string PayloadJson { get; set; } = "{}";

        public string? CorrelationId { get; set; }
    }


}
