using AiIncidentResponseAgent.Domain.Common;

namespace AiIncidentResponseAgent.Domain.Auth;

public sealed class RefreshToken : Entity
{
    private RefreshToken() { }

    public RefreshToken(
        Guid userId,
        string tokenHash,
        DateTime expiresAtUtc)
    {
        UserId = userId;
        TokenHash = tokenHash;
        ExpiresAtUtc = expiresAtUtc;
        IsRevoked = false;
    }

    public Guid UserId { get; private set; }
    public string TokenHash { get; private set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; private set; }
    public bool IsRevoked { get; private set; }
    public DateTime? RevokedAtUtc { get; private set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAtUtc;
    public bool IsActive => !IsRevoked && !IsExpired;

    public void Revoke()
    {
        if (IsRevoked)
        {
            return;
        }

        IsRevoked = true;
        RevokedAtUtc = DateTime.UtcNow;
    }
}