using AiIncidentResponseAgent.Domain.Auth;

namespace AiIncidentResponseAgent.Application.Abstractions
{
    public interface IJwtTokenService
    {
        string CreateToken(AuthUser user, DateTime expiresAtUtc);
    }
}
