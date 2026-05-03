using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AiIncidentResponseAgent.Application.Abstractions;
using AiIncidentResponseAgent.Application.Abstractions.Repositories;
using AiIncidentResponseAgent.Application.Actions;
using AiIncidentResponseAgent.Infrastructure.Actions;
using AiIncidentResponseAgent.Infrastructure.Actions.Handlers;
using AiIncidentResponseAgent.Infrastructure.Ai;
using AiIncidentResponseAgent.Infrastructure.Auth;
using AiIncidentResponseAgent.Infrastructure.Persistence;
using AiIncidentResponseAgent.Infrastructure.Persistence.Repositories;
using AiIncidentResponseAgent.Infrastructure.Realtime;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using static AiIncidentResponseAgent.Infrastructure.Ai.StubAgentAnalyzer;

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

        services.AddScoped<ITicketRepository, TicketRepository>();
        services.AddScoped<IAuthUserRepository, AuthUserRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();

        services.AddScoped<IAgentMetricsRepository, AgentMetricsRepository>();



        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));

        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        /*
        services.Configure<OpenAiAnalyzerOptions>(
    configuration.GetSection("OpenAiAnalyzer"));

        services.AddScoped<StubAgentAnalyzer>();
        services.AddScoped<IAgentAnalyzer, OpenAiAgentAnalyzer>();*/


        services.Configure<OllamaAnalyzerOptions>(
    configuration.GetSection("OllamaAnalyzer"));

        services.AddScoped<StubAgentAnalyzer>();

        services.AddHttpClient<IAgentAnalyzer, OllamaAgentAnalyzer>((sp, client) =>
        {
            var options = sp.GetRequiredService<
                Microsoft.Extensions.Options.IOptions<OllamaAnalyzerOptions>>().Value;

            client.BaseAddress = new Uri(options.BaseUrl);
        });

        services.AddHttpClient<ITextTranslator, OllamaTextTranslator>((sp, client) =>
        {
            var options = sp.GetRequiredService<
                Microsoft.Extensions.Options.IOptions<OllamaAnalyzerOptions>>().Value;

            client.BaseAddress = new Uri(options.BaseUrl);
        });





        services.AddScoped<IAgentActionExecutor, AgentActionExecutor>();

        services.AddScoped<IAgentActionHandler, BlockTicketActionHandler>();
        services.AddScoped<IAgentActionHandler, CreateIncidentActionHandler>();
        services.AddScoped<IAgentActionHandler, SendNotificationActionHandler>();

        services.AddScoped<IAgentActionHandler, EscalateActionHandler>();

        //services.TryAddScoped<IRealtimeNotifier, NoOpRealtimeNotifier>();

        services.Configure<RealtimeOptions>(
    configuration.GetSection("Realtime"));

        services.TryAddScoped<IRealtimeNotifier, NoOpRealtimeNotifier>();

        services.AddHttpClient<HttpRealtimeNotifier>((sp, client) =>
        {
            var options = sp.GetRequiredService<
                Microsoft.Extensions.Options.IOptions<RealtimeOptions>>().Value;

            client.BaseAddress = new Uri(options.ApiBaseUrl);
        });

        services.Configure<InternalApiKeyOptions>(
    configuration.GetSection("InternalApiKey"));


        return services;
    }
}
