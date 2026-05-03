using AiIncidentResponseAgent.Application.Abstractions.Repositories;
using AiIncidentResponseAgent.Domain.Auth;

using Microsoft.EntityFrameworkCore;

namespace AiIncidentResponseAgent.Infrastructure.Persistence.Repositories;

public sealed class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly AgentDbContext _db;

    public RefreshTokenRepository(AgentDbContext db)
    {
        _db = db;
    }

    public Task<RefreshToken?> GetByTokenHashAsync(
        string tokenHash,
        CancellationToken cancellationToken = default)
    {
        return _db.RefreshTokens.FirstOrDefaultAsync(
            x => x.TokenHash == tokenHash,
            cancellationToken);
    }

    public async Task AddAsync(
        RefreshToken refreshToken,
        CancellationToken cancellationToken = default)
    {
        await _db.RefreshTokens.AddAsync(refreshToken, cancellationToken);
    }
}