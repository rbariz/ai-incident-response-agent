using AiIncidentResponseAgent.Application.Models;

namespace AiIncidentResponseAgent.Application.Services;

public static class RetryBackoffCalculator
{
    public static DateTime CalculateNextRetryUtc(
        int retryCount,
        RetryOptions options)
    {
        var exponent = Math.Max(0, retryCount);
        var delaySeconds = options.BaseDelaySeconds * Math.Pow(2, exponent);

        delaySeconds = Math.Min(delaySeconds, options.MaxDelaySeconds);

        return DateTime.UtcNow.AddSeconds(delaySeconds);
    }
}
