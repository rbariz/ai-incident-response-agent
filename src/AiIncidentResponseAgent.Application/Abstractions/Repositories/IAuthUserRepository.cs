using AiIncidentResponseAgent.Domain.Auth;

namespace AiIncidentResponseAgent.Application.Abstractions.Repositories
{
    public interface IAuthUserRepository
    {
        Task<AuthUser?> GetByUsernameAsync(
            string username,
            CancellationToken cancellationToken = default);

        Task AddAsync(
            AuthUser user,
            CancellationToken cancellationToken = default);
    }
}
