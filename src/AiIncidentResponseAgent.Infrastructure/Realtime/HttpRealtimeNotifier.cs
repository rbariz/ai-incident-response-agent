using System.Net.Http.Json;

using AiIncidentResponseAgent.Application.Abstractions;
using AiIncidentResponseAgent.Contracts.Realtime;

using Microsoft.Extensions.Logging;

using Microsoft.Extensions.Options;

namespace AiIncidentResponseAgent.Infrastructure.Realtime;

public sealed class HttpRealtimeNotifier : IRealtimeNotifier
{
    private readonly HttpClient _httpClient;
    private readonly RealtimeOptions _options;
    private readonly ILogger<HttpRealtimeNotifier> _logger;
    private readonly InternalApiKeyOptions _internalApiKeyOptions;

    public HttpRealtimeNotifier(
        HttpClient httpClient,
        IOptions<RealtimeOptions> options,
        IOptions<InternalApiKeyOptions> internalApiKeyOptions,
        ILogger<HttpRealtimeNotifier> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _internalApiKeyOptions = internalApiKeyOptions.Value;
        _logger = logger;
    }

    public Task AgentEventCreatedAsync(Guid eventId, string correlationId, CancellationToken cancellationToken = default)
    {
        return BroadcastAsync("AgentEventCreated", new { eventId, correlationId }, cancellationToken);
    }

    public Task AgentExecutionStartedAsync(Guid executionId, Guid eventId, string correlationId, CancellationToken cancellationToken = default)
    {
        return BroadcastAsync("AgentExecutionStarted", new { executionId, eventId, correlationId }, cancellationToken);
    }

    public Task AgentExecutionCompletedAsync(Guid executionId, Guid eventId, string status, string action, string correlationId, CancellationToken cancellationToken = default)
    {
        return BroadcastAsync("AgentExecutionCompleted", new
        {
            executionId,
            eventId,
            status,
            action,
            correlationId
        }, cancellationToken);
    }

    public Task IncidentChangedAsync(Guid incidentId, Guid eventId, string status, string severity, CancellationToken cancellationToken = default)
    {
        return BroadcastAsync("IncidentChanged", new
        {
            incidentId,
            eventId,
            status,
            severity
        }, cancellationToken);
    }

    private async Task BroadcastAsync(string eventName, object payload, CancellationToken cancellationToken)
    {
        if (!_options.Enabled)
        {
            return;
        }

        try
        {
            var request = new RealtimeEventRequest
            {
                EventName = eventName,
                Payload = payload
            };

            _httpClient.DefaultRequestHeaders.Remove("X-Internal-Api-Key");
            _httpClient.DefaultRequestHeaders.Add(
                "X-Internal-Api-Key",
                _internalApiKeyOptions.ApiKey);

            using var response = await _httpClient.PostAsJsonAsync(
                "/api/internal/realtime/broadcast",
                request,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Realtime broadcast failed. Event={EventName}, Status={StatusCode}",
                    eventName,
                    response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Realtime broadcast failed. Event={EventName}", eventName);
        }
    }

    public Task AgentExecutionApprovalChangedAsync(
    Guid executionId,
    string status,
    string reason,
    CancellationToken cancellationToken = default)
    {
        return BroadcastAsync("AgentExecutionApprovalChanged", new
        {
            executionId,
            status,
            reason
        }, cancellationToken);
    }
}
