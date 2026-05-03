using AiIncidentResponseAgent.Application.Abstractions;
using AiIncidentResponseAgent.Application.Abstractions.Repositories;
using AiIncidentResponseAgent.Contracts.Auth;
using AiIncidentResponseAgent.Infrastructure.Auth;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AiIncidentResponseAgent.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthUserRepository _users;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwt;
    private readonly JwtOptions _jwtOptions;

    public AuthController(
        IAuthUserRepository users,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwt,
        IOptions<JwtOptions> jwtOptions)
    {
        _users = users;
        _passwordHasher = passwordHasher;
        _jwt = jwt;
        _jwtOptions = jwtOptions.Value;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var username = request.Username.Trim();

        var user = await _users.GetByUsernameAsync(
            username,
            cancellationToken);

        if (user is null || !user.IsActive)
        {
            return Unauthorized("Invalid credentials.");
        }

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            return Unauthorized("Invalid credentials.");
        }

        var expiresAtUtc = DateTime.UtcNow.AddMinutes(
            _jwtOptions.ExpirationMinutes);

        return Ok(new LoginResponse
        {
            AccessToken = _jwt.CreateToken(user, expiresAtUtc),
            Username = user.Username,
            Role = user.Role.ToString(),
            ExpiresAtUtc = expiresAtUtc
        });
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        return Ok(new
        {
            username = User.Identity?.Name,
            role = User.Claims.FirstOrDefault(x => x.Type.EndsWith("role"))?.Value
        });
    }
}