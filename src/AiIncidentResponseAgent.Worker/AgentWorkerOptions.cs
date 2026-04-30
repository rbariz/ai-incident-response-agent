using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiIncidentResponseAgent.Worker;
public sealed class AgentWorkerOptions
{
    public int PollingIntervalSeconds { get; set; } = 5;
    public int BatchSize { get; set; } = 20;
}
