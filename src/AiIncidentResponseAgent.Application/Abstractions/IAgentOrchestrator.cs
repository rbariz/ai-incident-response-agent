using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiIncidentResponseAgent.Application.Abstractions
{
    public interface IAgentOrchestrator
    {
        Task ProcessEventAsync(Guid eventId, CancellationToken cancellationToken = default);
    }
}
