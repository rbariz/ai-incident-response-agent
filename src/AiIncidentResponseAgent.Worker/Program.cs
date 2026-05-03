using AiIncidentResponseAgent.Application;
using AiIncidentResponseAgent.Application.Abstractions;
using AiIncidentResponseAgent.Infrastructure;
using AiIncidentResponseAgent.Infrastructure.Realtime;
using AiIncidentResponseAgent.Worker;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<AgentWorkerOptions>(
    builder.Configuration.GetSection("AgentWorker"));

builder.Services.AddApplication(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<IRealtimeNotifier, HttpRealtimeNotifier>();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();

await host.RunAsync();