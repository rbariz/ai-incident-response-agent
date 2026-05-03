using AiIncidentResponseAgent.Infrastructure.Ai;

using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace AiIncidentResponseAgent.Api.Health;

public sealed class OllamaHealthCheck : IHealthCheck
{
    private readonly HttpClient _httpClient;
    private readonly OllamaAnalyzerOptions _options;

    public OllamaHealthCheck(
        HttpClient httpClient,
        IOptions<OllamaAnalyzerOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            return HealthCheckResult.Healthy("Ollama analyzer is disabled.");
        }

        try
        {
            using var response = await _httpClient.GetAsync(
                "/api/tags",
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return HealthCheckResult.Unhealthy(
                    $"Ollama returned HTTP {(int)response.StatusCode}.");
            }

            return HealthCheckResult.Healthy("Ollama is reachable.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Ollama is unreachable.", ex);
        }
    }
}
