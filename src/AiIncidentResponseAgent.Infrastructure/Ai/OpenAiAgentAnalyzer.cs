using System.Text.Json;

using AiIncidentResponseAgent.Application.Abstractions;
using AiIncidentResponseAgent.Application.Models;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using OpenAI.Chat;

namespace AiIncidentResponseAgent.Infrastructure.Ai;

public sealed partial class StubAgentAnalyzer
{
    public sealed class OpenAiAgentAnalyzer : IAgentAnalyzer
{
    private readonly OpenAiAnalyzerOptions _options;
    private readonly StubAgentAnalyzer _fallback;
    private readonly ILogger<OpenAiAgentAnalyzer> _logger;

    public OpenAiAgentAnalyzer(
        IOptions<OpenAiAnalyzerOptions> options,
        StubAgentAnalyzer fallback,
        ILogger<OpenAiAgentAnalyzer> logger)
    {
        _options = options.Value;
        _fallback = fallback;
        _logger = logger;
    }

    public async Task<AgentAnalysisResult> AnalyzeAsync(
        AgentContext context,
        CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled || string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            return await _fallback.AnalyzeAsync(context, cancellationToken);
        }

        try
        {
            var client = new ChatClient(_options.Model, _options.ApiKey);

            var systemPrompt = """
            You are an AI incident response analyzer.
            Your role is to analyze operational events.
            You must not decide final actions.
            You must only summarize, classify intent, estimate confidence, and suggest an action.
            Return JSON only.
            """;

            var userPrompt = $$"""
            EventType: {{context.Event.Type}}
            Source: {{context.Event.Source}}
            CorrelationId: {{context.Event.CorrelationId}}
            PayloadJson: {{context.Event.PayloadJson}}
            MemoryJson: {{context.MemoryJson}}

            Allowed suggestedAction values:
            - None
            - BlockTicket
            - RestartService
            - SendNotification
            - CreateIncident
            - Escalate
            - Retry
            """;

            ChatCompletion completion = await client.CompleteChatAsync(
                [
                    new SystemChatMessage(systemPrompt),
                    new UserChatMessage(userPrompt)
                ],
                cancellationToken: cancellationToken);

            var json = completion.Content[0].Text;

            var parsed = JsonSerializer.Deserialize<OpenAiAgentAnalysisResponse>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            if (parsed is null)
            {
                return await _fallback.AnalyzeAsync(context, cancellationToken);
            }

            return new AgentAnalysisResult
            {
                Summary = parsed.Summary,
                Intent = parsed.Intent,
                ConfidenceScore = Math.Clamp(parsed.ConfidenceScore, 0m, 1m),
                SuggestedAction = parsed.SuggestedAction,
                RawResponseJson = json
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OpenAI analyzer failed. Falling back to stub analyzer.");
            return await _fallback.AnalyzeAsync(context, cancellationToken);
        }
    }
}
}
