using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using AiIncidentResponseAgent.Application.Abstractions;
using AiIncidentResponseAgent.Domain.Auth;
using AiIncidentResponseAgent.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AiIncidentResponseAgent.Infrastructure.Auth;

public static class AuthSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<AgentDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        if (await db.AuthUsers.AnyAsync())
        {
            return;
        }

        db.AuthUsers.AddRange(
            new AuthUser("admin", hasher.Hash("Admin123!"), AuthRole.Admin),
            new AuthUser("operator", hasher.Hash("Operator123!"), AuthRole.Operator),
            new AuthUser("viewer", hasher.Hash("Viewer123!"), AuthRole.Viewer)
        );

        await db.SaveChangesAsync();
    }
}
