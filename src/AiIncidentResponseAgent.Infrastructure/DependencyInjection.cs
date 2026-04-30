using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AiIncidentResponseAgent.Application.Abstractions;
using AiIncidentResponseAgent.Application.Abstractions.Repositories;
using AiIncidentResponseAgent.Infrastructure.Actions;
using AiIncidentResponseAgent.Infrastructure.Actions.Handlers;
using AiIncidentResponseAgent.Infrastructure.Ai;
using AiIncidentResponseAgent.Infrastructure.Persistence;
using AiIncidentResponseAgent.Infrastructure.Persistence.Repositories;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AiIncidentResponseAgent.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AgentDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
        });

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AgentDbContext>());

        services.AddScoped<IAgentEventRepository, AgentEventRepository>();
        services.AddScoped<IAgentExecutionRepository, AgentExecutionRepository>();
        services.AddScoped<IIncidentRepository, IncidentRepository>();
        services.AddScoped<IAgentMemoryRepository, AgentMemoryRepository>();
        services.AddScoped<IAgentActionLockRepository, AgentActionLockRepository>();

        services.AddScoped<IAgentAnalyzer, StubAgentAnalyzer>();
        services.AddScoped<IAgentActionExecutor, AgentActionExecutor>();

        services.AddScoped<IAgentActionHandler, BlockTicketActionHandler>();
        services.AddScoped<IAgentActionHandler, CreateIncidentActionHandler>();
        services.AddScoped<IAgentActionHandler, SendNotificationActionHandler>();

        return services;
    }
}
