using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AiIncidentResponseAgent.Infrastructure.Persistence;
public class AgentDbContextFactory : IDesignTimeDbContextFactory<AgentDbContext>
{
    public AgentDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AgentDbContext>();

        optionsBuilder.UseNpgsql(
            "Host=localhost;Port=5432;Database=ai_agent_v1;Username=postgres;Password=postgres");

        return new AgentDbContext(optionsBuilder.Options);
    }
}