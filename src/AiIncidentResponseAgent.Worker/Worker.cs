using AiIncidentResponseAgent.Application.Abstractions;
using AiIncidentResponseAgent.Application.Abstractions.Repositories;

using Microsoft.Extensions.Options;

namespace AiIncidentResponseAgent.Worker;

public sealed class Worker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<Worker> _logger;
    private readonly AgentWorkerOptions _options;

    public Worker(
        IServiceScopeFactory scopeFactory,
        ILogger<Worker> logger,
        IOptions<AgentWorkerOptions> options)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("AI Incident Response Agent Worker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessBatchAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Worker polling cycle failed.");
            }

            await Task.Delay(
                TimeSpan.FromSeconds(_options.PollingIntervalSeconds),
                stoppingToken);
        }
    }

    private async Task ProcessBatchAsync(CancellationToken cancellationToken)
    {

        _logger.LogInformation(
    "Worker polling cycle started. BatchSize={BatchSize}",
    _options.BatchSize);
        using var scope = _scopeFactory.CreateScope();

        var events = scope.ServiceProvider.GetRequiredService<IAgentEventRepository>();
        var orchestrator = scope.ServiceProvider.GetRequiredService<IAgentOrchestrator>();

        var unprocessedEvents = await events.GetUnprocessedAsync(
            _options.BatchSize,
            cancellationToken);

        if (unprocessedEvents.Count == 0)
        {
            _logger.LogDebug("No unprocessed agent events found.");
        }
        else
        {
            _logger.LogInformation(
                "Found {Count} unprocessed agent events.",
                unprocessedEvents.Count);

            foreach (var agentEvent in unprocessedEvents)
            {
                using var eventScope = _logger.BeginScope(new Dictionary<string, object>
                {
                    ["EventId"] = agentEvent.Id,
                    ["CorrelationId"] = agentEvent.CorrelationId,
                    ["EventType"] = agentEvent.Type.ToString()
                });

                try
                {
                    _logger.LogInformation("Processing agent event.");

                    await orchestrator.ProcessEventAsync(
                        agentEvent.Id,
                        cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Failed to process agent event.");
                }
            }
        }

        _logger.LogInformation(
            "Found {Count} unprocessed agent events.",
            unprocessedEvents.Count);

        foreach (var agentEvent in unprocessedEvents)
        {
            try
            {
                await orchestrator.ProcessEventAsync(agentEvent.Id, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to process agent event {EventId}.",
                    agentEvent.Id);
            }
        }

        var retryProcessor = scope.ServiceProvider.GetRequiredService<IAgentRetryProcessor>();

        _logger.LogInformation("Worker retry cycle started.");

        await retryProcessor.ProcessRetriesAsync(
            _options.BatchSize,
            cancellationToken);

        _logger.LogInformation("Worker retry cycle completed.");
    }
}