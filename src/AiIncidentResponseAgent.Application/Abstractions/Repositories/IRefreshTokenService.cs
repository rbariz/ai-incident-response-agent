namespace AiIncidentResponseAgent.Application.Abstractions.Repositories
{
    public interface IRefreshTokenService
    {
        string GenerateRawToken();
        string HashToken(string rawToken);
    }
}
