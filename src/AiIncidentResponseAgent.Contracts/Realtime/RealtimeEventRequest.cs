using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiIncidentResponseAgent.Contracts.Realtime;

public sealed class RealtimeEventRequest
{
    public string EventName { get; set; } = string.Empty;

    public object Payload { get; set; } = new();
}
