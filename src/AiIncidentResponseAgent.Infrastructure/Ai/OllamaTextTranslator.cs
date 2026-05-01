using System.Net.Http.Json;

using AiIncidentResponseAgent.Application.Abstractions;

using Microsoft.Extensions.Options;

namespace AiIncidentResponseAgent.Infrastructure.Ai;

public sealed class OllamaTextTranslator : ITextTranslator
{
    private readonly HttpClient _httpClient;
    private readonly OllamaAnalyzerOptions _options;

    public OllamaTextTranslator(
        HttpClient httpClient,
        IOptions<OllamaAnalyzerOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<string> TranslateAsync(
        string text,
        string fromLang,
        string toLang,
        CancellationToken cancellationToken = default)
    {
        fromLang = NormalizeLang(fromLang);
        toLang = NormalizeLang(toLang);

        if (string.IsNullOrWhiteSpace(text) || fromLang == toLang)
        {
            return text;
        }

        var fromLanguage = fromLang == "fr" ? "French" : "English";
        var toLanguage = toLang == "fr" ? "French" : "English";

        var request = new
        {
            model = _options.Model,
            stream = false,
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
                    content = """
                    You are a professional translation engine.
                    Translate the text accurately.
                    Return only the translated text.
                    Do not add explanations.
                    Do not add quotes.
                    Do not add markdown.
                    Keep operational/technical meaning precise.
                    """
                },
                new
                {
                    role = "user",
                    content = $$"""
                    Translate from {{fromLanguage}} to {{toLanguage}}:

                    {{text}}
                    """
                }
            }
        };

        using var response = await _httpClient.PostAsJsonAsync(
            "/api/chat",
            request,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<OllamaChatResponse>(
            cancellationToken: cancellationToken);

        return result?.Message.Content?.Trim() ?? text;
    }

    private static string NormalizeLang(string? lang)
    {
        return string.Equals(lang, "fr", StringComparison.OrdinalIgnoreCase)
            ? "fr"
            : "en";
    }
}