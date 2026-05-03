using System.Security.Cryptography;
using System.Text;

using AiIncidentResponseAgent.Application.Abstractions.Repositories;

namespace AiIncidentResponseAgent.Infrastructure.Auth;

public sealed class RefreshTokenService : IRefreshTokenService
{
    public string GenerateRawToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    }

    public string HashToken(string rawToken)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToHexString(bytes);
    }
}
