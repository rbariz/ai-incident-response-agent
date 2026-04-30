using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AiIncidentResponseAgent.Application.Abstractions;
using AiIncidentResponseAgent.Domain.Actions;
using AiIncidentResponseAgent.Domain.Events;
using AiIncidentResponseAgent.Domain.Executions;
using AiIncidentResponseAgent.Domain.Incidents;
using AiIncidentResponseAgent.Domain.Memory;

using Microsoft.EntityFrameworkCore;

namespace AiIncidentResponseAgent.Infrastructure.Persistence
{
    public sealed class AgentDbContext : DbContext, IUnitOfWork
    {
        public AgentDbContext(DbContextOptions<AgentDbContext> options)
            : base(options)
        {
        }

        public DbSet<AgentEvent> AgentEvents => Set<AgentEvent>();
        public DbSet<AgentExecution> AgentExecutions => Set<AgentExecution>();
        public DbSet<AgentMemory> AgentMemories => Set<AgentMemory>();
        public DbSet<Incident> Incidents => Set<Incident>();

        public DbSet<AgentActionLock> AgentActionLocks => Set<AgentActionLock>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AgentDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }
    }
}
