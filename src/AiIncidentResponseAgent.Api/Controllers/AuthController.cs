using AiIncidentResponseAgent.Application.Abstractions;
using AiIncidentResponseAgent.Application.Abstractions.Repositories;
using AiIncidentResponseAgent.Contracts.Auth;
using AiIncidentResponseAgent.Domain.Auth;
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

    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly IUnitOfWork _unitOfWork;

    public AuthController(
        IAuthUserRepository users,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwt,
        IOptions<JwtOptions> jwtOptions,
        IRefreshTokenService refreshTokenService,
        IRefreshTokenRepository refreshTokens,
        IUnitOfWork unitOfWork)
    {
        _users = users;
        _passwordHasher = passwordHasher;
        _jwt = jwt;
        _jwtOptions = jwtOptions.Value;
        _refreshTokenService = refreshTokenService;
        _refreshTokens = refreshTokens;
        _unitOfWork = unitOfWork;
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

        var rawRefreshToken = _refreshTokenService.GenerateRawToken();
        var refreshTokenHash = _refreshTokenService.HashToken(rawRefreshToken);

        var refreshToken = new RefreshToken(
            user.Id,
            refreshTokenHash,
            DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpirationDays));

        await _refreshTokens.AddAsync(refreshToken, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Ok(new LoginResponse
        {
            AccessToken = _jwt.CreateToken(user, expiresAtUtc),
            Username = user.Username,
            Role = user.Role.ToString(),
            ExpiresAtUtc = expiresAtUtc,
            RefreshToken = rawRefreshToken
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


    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<ActionResult<LoginResponse>> Refresh(
    [FromBody] RefreshTokenRequest request,
    CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return BadRequest("Refresh token is required.");
        }

        var tokenHash = _refreshTokenService.HashToken(request.RefreshToken);

        var storedToken = await _refreshTokens.GetByTokenHashAsync(
            tokenHash,
            cancellationToken);

        if (storedToken is null || !storedToken.IsActive)
        {
            return Unauthorized("Invalid refresh token.");
        }

        var user = await _users.GetByIdAsync(
            storedToken.UserId,
            cancellationToken);

        if (user is null || !user.IsActive)
        {
            return Unauthorized("Invalid refresh token.");
        }

        storedToken.Revoke();

        var expiresAtUtc = DateTime.UtcNow.AddMinutes(
            _jwtOptions.ExpirationMinutes);

        var newRawRefreshToken = _refreshTokenService.GenerateRawToken();
        var newRefreshTokenHash = _refreshTokenService.HashToken(newRawRefreshToken);

        var newRefreshToken = new RefreshToken(
            user.Id,
            newRefreshTokenHash,
            DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpirationDays));

        await _refreshTokens.AddAsync(newRefreshToken, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Ok(new LoginResponse
        {
            AccessToken = _jwt.CreateToken(user, expiresAtUtc),
            RefreshToken = newRawRefreshToken,
            Username = user.Username,
            Role = user.Role.ToString(),
            ExpiresAtUtc = expiresAtUtc
        });
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(
    [FromBody] RefreshTokenRequest request,
    CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return NoContent();
        }

        var tokenHash = _refreshTokenService.HashToken(request.RefreshToken);

        var storedToken = await _refreshTokens.GetByTokenHashAsync(
            tokenHash,
            cancellationToken);

        if (storedToken is not null && storedToken.IsActive)
        {
            storedToken.Revoke();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return NoContent();
    }
}