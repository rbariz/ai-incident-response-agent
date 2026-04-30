using AiIncidentResponseAgent.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiIncidentResponseAgent.Domain.Memory
{
    public sealed class AgentMemory : Entity
    {
        private AgentMemory() { }

        public AgentMemory(
            string entityId,
            string entityType,
            string contextJson)
        {
            EntityId = entityId;
            EntityType = entityType;
            ContextJson = contextJson;
            LastUpdatedAtUtc = DateTime.UtcNow;
        }

        public string EntityId { get; private set; } = string.Empty;
        public string EntityType { get; private set; } = string.Empty;
        public string ContextJson { get; private set; } = "{}";
        public DateTime LastUpdatedAtUtc { get; private set; }

        public void UpdateContext(string contextJson)
        {
            ContextJson = contextJson;
            LastUpdatedAtUtc = DateTime.UtcNow;
        }
    }
}
