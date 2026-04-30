using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiIncidentResponseAgent.Domain.Autonomy
{
    public enum AutonomyLevel
    {
        Low = 1,       // Observe only
        Medium = 2,    // Suggest action
        High = 3,      // Execute action
        Critical = 4   // Execute + escalate
    }
}
