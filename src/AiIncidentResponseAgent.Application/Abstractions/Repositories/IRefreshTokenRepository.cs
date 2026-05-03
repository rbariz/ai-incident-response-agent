using AiIncidentResponseAgent.Domain.Auth;

namespace AiIncidentResponseAgent.Application.Abstractions.Repositories
{
    public interface IRefreshTokenRepository
    {
        Task<RefreshToken?> GetByTokenHashAsync(
            string tokenHash,
            CancellationToken cancellationToken = default);

        Task AddAsync(
            RefreshToken refreshToken,
            CancellationToken cancellationToken = default);
    }
}
