using AiIncidentResponseAgent.Domain.Common;

namespace AiIncidentResponseAgent.Domain.Auth;

public sealed class AuthUser : Entity
{
    private AuthUser() { }

    public AuthUser(
        string username,
        string passwordHash,
        AuthRole role)
    {
        Username = username;
        PasswordHash = passwordHash;
        Role = role;
        IsActive = true;
    }

    public string Username { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public AuthRole Role { get; private set; }
    public bool IsActive { get; private set; }

    public void Deactivate() => IsActive = false;
}