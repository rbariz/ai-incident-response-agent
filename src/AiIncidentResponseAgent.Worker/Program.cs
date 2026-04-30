using AiIncidentResponseAgent.Application;
using AiIncidentResponseAgent.Infrastructure;
using AiIncidentResponseAgent.Worker;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<AgentWorkerOptions>(
    builder.Configuration.GetSection("AgentWorker"));

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddHostedService<Worker>();

var host = builder.Build();

await host.RunAsync();