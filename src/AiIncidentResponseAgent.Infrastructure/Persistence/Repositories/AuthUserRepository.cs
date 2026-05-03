using AiIncidentResponseAgent.Application.Abstractions.Repositories;
using AiIncidentResponseAgent.Domain.Auth;

using Microsoft.EntityFrameworkCore;

namespace AiIncidentResponseAgent.Infrastructure.Persistence.Repositories;

public sealed class AuthUserRepository : IAuthUserRepository
{
    private readonly AgentDbContext _db;

    public AuthUserRepository(AgentDbContext db)
    {
        _db = db;
    }

    public Task<AuthUser?> GetByUsernameAsync(
        string username,
        CancellationToken cancellationToken = default)
    {
        return _db.AuthUsers.FirstOrDefaultAsync(
            x => x.Username == username,
            cancellationToken);
    }

    public async Task AddAsync(
        AuthUser user,
        CancellationToken cancellationToken = default)
    {
        await _db.AuthUsers.AddAsync(user, cancellationToken);
    }
}
