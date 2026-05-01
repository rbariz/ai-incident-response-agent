namespace AiIncidentResponseAgent.Application.Abstractions
{
    public interface ITextTranslator
    {
        Task<string> TranslateAsync(
            string text,
            string fromLang,
            string toLang,
            CancellationToken cancellationToken = default);
    }
}
