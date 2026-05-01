using System.Net.Http.Json;
using System.Text.Json;

using AiIncidentResponseAgent.Application.Abstractions;
using AiIncidentResponseAgent.Application.Models;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using static AiIncidentResponseAgent.Infrastructure.Ai.StubAgentAnalyzer;

namespace AiIncidentResponseAgent.Infrastructure.Ai;

public sealed class OllamaAgentAnalyzer : IAgentAnalyzer
{
    private readonly HttpClient _httpClient;
    private readonly OllamaAnalyzerOptions _options;
    private readonly StubAgentAnalyzer _fallback;
    private readonly ILogger<OllamaAgentAnalyzer> _logger;

    public OllamaAgentAnalyzer(
        HttpClient httpClient,
        IOptions<OllamaAnalyzerOptions> options,
        StubAgentAnalyzer fallback,
        ILogger<OllamaAgentAnalyzer> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _fallback = fallback;
        _logger = logger;
    }

    public async Task<AgentAnalysisResult> AnalyzeAsync(
        AgentContext context,
        CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            return await _fallback.AnalyzeAsync(context, cancellationToken);
        }

        for (var attempt = 1; attempt <= 2; attempt++)
        {
            try
            {
                var rawJson = await AskOllamaAsync(context, attempt, context.Lang, cancellationToken);

                var parsed = JsonSerializer.Deserialize<OpenAiAgentAnalysisResponse>(
                    rawJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (AgentAnalysisValidator.IsValid(parsed))
                {
                    _logger.LogInformation(
                        "Ollama analysis succeeded for event {EventId} on attempt {Attempt}.",
                        context.Event.Id,
                        attempt);

                    return AgentAnalysisValidator.ToResult(
                        parsed!,
                        rawJson,
                        "ollama");
                }

                _logger.LogWarning(
                    "Invalid Ollama analysis for event {EventId} on attempt {Attempt}. Raw: {RawJson}",
                    context.Event.Id,
                    attempt,
                    rawJson);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Ollama analysis failed for event {EventId} on attempt {Attempt}.",
                    context.Event.Id,
                    attempt);
            }
        }

        _logger.LogWarning(
            "Falling back to stub analyzer for event {EventId}.",
            context.Event.Id);

        return await _fallback.AnalyzeAsync(context, cancellationToken);
    }

    private async Task<string> AskOllamaAsync(
    AgentContext context,
    int attempt,
    string lang,
    CancellationToken cancellationToken)
    {
        var normalizedLang = NormalizeLang(lang);
        var summaryLanguage = normalizedLang == "fr" ? "French" : "English";

        var request = new
        {
            model = _options.Model,
            stream = false,
            format = "json",
            options = new
            {
                temperature = 0,
                top_p = 0.1
            },
            messages = new object[]
            {
            new
            {
                role = "system",
                content = $$"""
                You are an AI Incident Response Analyzer for an autonomous operations platform.

                Your role:
                - Analyze operational events.
                - Produce a short structured interpretation.
                - Suggest a possible action.
                - NEVER decide final execution.
                - NEVER claim that an action was executed.
                - NEVER invent missing facts.

                The final decision is made by a deterministic Decision Engine and Safety Policy Engine.

                Return ONLY valid JSON.
                No markdown.
                No comments.
                No explanations outside JSON.

                Required JSON schema:
                {
                  "summary": "string",
                  "intent": "Antifraud | SystemMonitoring | FraudRisk | BusinessAnomaly | Unknown",
                  "confidenceScore": 0.0,
                  "suggestedAction": "None | BlockTicket | RestartService | SendNotification | CreateIncident | Escalate | Retry"
                }

                Language rule:
                - The "summary" field must be written in {{summaryLanguage}}.
                - All enum fields must remain in English exactly as defined:
                  intent, suggestedAction.

                Rules:
                1. summary must be short, factual, and operational.
                2. intent must be one of the allowed values only.
                3. confidenceScore must be between 0 and 1.
                4. suggestedAction must be one of the allowed values only.
                5. If event type or payload is unclear, use:
                   - intent = "Unknown"
                   - confidenceScore <= 0.40
                   - suggestedAction = "None"
                6. Do not suggest RestartService unless the event clearly indicates system/API instability.
                7. Do not suggest BlockTicket unless the event clearly indicates ticket fraud or duplicate scan.
                8. Do not suggest Escalate unless human review is clearly needed.
                9. Prefer CreateIncident for suspicious but uncertain cases.
                10. Never output confidenceScore = 1.0.

                Examples:

                Input:
                EventType: DuplicateScan
                PayloadJson: {"ticketId":"TCK-001","gate":"A1"}
                Output:
                {
                  "summary": "Duplicate ticket scan detected for ticket TCK-001 at gate A1.",
                  "intent": "Antifraud",
                  "confidenceScore": 0.92,
                  "suggestedAction": "BlockTicket"
                }

                Input:
                EventType: Unknown
                PayloadJson: {}
                Output:
                {
                  "summary": "Unknown event received with insufficient operational context.",
                  "intent": "Unknown",
                  "confidenceScore": 0.25,
                  "suggestedAction": "None"
                }
                """
            },
            new
            {
                role = "user",
                content = $$"""
                Analyze the following operational event.

                RequestedSummaryLanguage: {{summaryLanguage}}
                EventType: {{context.Event.Type}}
                Source: {{context.Event.Source}}
                CorrelationId: {{context.Event.CorrelationId}}
                PayloadJson: {{context.Event.PayloadJson}}
                MemoryJson: {{context.MemoryJson}}

                Attempt: {{attempt}}

                Return only the required JSON object.
                """
            }
            }
        };

        using var response = await _httpClient.PostAsJsonAsync(
            "/api/chat",
            request,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var ollamaResponse = await response.Content.ReadFromJsonAsync<OllamaChatResponse>(
            cancellationToken: cancellationToken);

        var content = ollamaResponse?.Message.Content;

        if (string.IsNullOrWhiteSpace(content))
        {
            throw new InvalidOperationException("Ollama returned empty content.");
        }

        return ExtractJson(content);
    }

    private static string NormalizeLang(string? lang)
    {
        return string.Equals(lang, "fr", StringComparison.OrdinalIgnoreCase)
            ? "fr"
            : "en";
    }

    private static string ExtractJson(string content)
    {
        var start = content.IndexOf('{');
        var end = content.LastIndexOf('}');

        if (start < 0 || end <= start)
        {
            throw new InvalidOperationException("No JSON object found in Ollama response.");
        }

        return content[start..(end + 1)];
    }
}