namespace AiIncidentResponseAgent.Application.Abstractions
{
    public interface IAgentRetryProcessor
    {
        Task ProcessRetriesAsync(
            int take,
            CancellationToken cancellationToken = default);
    }
}
