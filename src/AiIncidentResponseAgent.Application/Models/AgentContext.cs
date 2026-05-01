using AiIncidentResponseAgent.Domain.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiIncidentResponseAgent.Application.Models
{
    public sealed class AgentContext
    {
        public required AgentEvent Event { get; init; }

        public string HistoryJson { get; init; } = "{}";

        public string MemoryJson { get; init; } = "{}";

        public string RiskContextJson { get; init; } = "{}";

        public string Lang { get; init; } = "en";
    }


}
