using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using server.DTOs.Auth;
using server.DTOs.Common;
using server.Extensions;
using server.Models;
using server.Data;
using server.Repositories.Interfaces;
using server.Services.Interfaces;

namespace server.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IUserRepository userRepository, IJwtTokenService jwtTokenService, AppDbContext db, IWebHostEnvironment environment) : ControllerBase
{
    private const string AccessTokenCookieName = "access_token";
    private const string RefreshTokenCookieName = "refresh_token";

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.Fail("Invalid payload."));
        }

        if (request.Role == UserRole.ADMIN)
        {
            return BadRequest(ApiResponse<object>.Fail("Admin registration is not allowed."));
        }

        if (await userRepository.EmailExistsAsync(request.Email, ct))
        {
            return Conflict(ApiResponse<object>.Fail("Email already exists."));
        }

        if (await userRepository.UsernameExistsAsync(request.Username, ct))
        {
            return Conflict(ApiResponse<object>.Fail("Username already exists."));
        }

        var user = new User
        {
            UserId = Guid.NewGuid(),
            Username = request.Username.Trim(),
            Email = request.Email.Trim().ToLowerInvariant(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Gender = request.Gender,
            Age = request.Age,
            Role = request.Role,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await userRepository.AddAsync(user, ct);

        var (token, expiresAt) = jwtTokenService.Generate(user);
        var refreshRawToken = GenerateRawToken();
        var refreshExpiry = DateTime.UtcNow.AddDays(1);

        db.RefreshTokens.Add(new RefreshToken
        {
            TokenId = Guid.NewGuid(),
            UserId = user.UserId,
            TokenHash = ComputeSha256(refreshRawToken),
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = refreshExpiry,
            IsRememberMe = false
        });
        await db.SaveChangesAsync(ct);

        WriteAuthCookies(token, expiresAt, refreshRawToken, refreshExpiry);

        var response = new AuthResponse
        {
            UserId = user.UserId,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role.ToString(),
            Token = token,
            ExpiresAtUtc = expiresAt
        };

        return Ok(ApiResponse<AuthResponse>.Ok(response, "Registration successful."));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.Fail("Invalid payload."));
        }

        var user = await userRepository.GetByEmailAsync(request.Email.Trim(), ct);
        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return Unauthorized(ApiResponse<object>.Fail("Invalid email or password."));
        }

        if (!user.IsActive)
        {
            return Forbid();
        }

        var (token, expiresAt) = jwtTokenService.Generate(user);
        var refreshRawToken = GenerateRawToken();
        var refreshExpiry = request.RememberMe
            ? DateTime.UtcNow.AddDays(30)
            : DateTime.UtcNow.AddDays(1);

        db.RefreshTokens.Add(new RefreshToken
        {
            TokenId = Guid.NewGuid(),
            UserId = user.UserId,
            TokenHash = ComputeSha256(refreshRawToken),
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = refreshExpiry,
            IsRememberMe = request.RememberMe
        });
        await db.SaveChangesAsync(ct);

        WriteAuthCookies(token, expiresAt, refreshRawToken, refreshExpiry);

        var response = new AuthResponse
        {
            UserId = user.UserId,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role.ToString(),
            Token = token,
            ExpiresAtUtc = expiresAt
        };

        return Ok(ApiResponse<AuthResponse>.Ok(response, "Login successful."));
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh(CancellationToken ct)
    {
        if (!Request.Cookies.TryGetValue(RefreshTokenCookieName, out var refreshTokenRaw) || string.IsNullOrWhiteSpace(refreshTokenRaw))
        {
            return Unauthorized(ApiResponse<object>.Fail("Refresh token not found."));
        }

        var tokenHash = ComputeSha256(refreshTokenRaw);
        var refreshToken = await db.RefreshTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash, ct);

        if (refreshToken is null || refreshToken.RevokedAt is not null || refreshToken.ExpiresAt <= DateTime.UtcNow || !refreshToken.User.IsActive)
        {
            return Unauthorized(ApiResponse<object>.Fail("Invalid refresh token."));
        }

        var (accessToken, accessExpiry) = jwtTokenService.Generate(refreshToken.User);
        var newRefreshRaw = GenerateRawToken();
        var newRefreshHash = ComputeSha256(newRefreshRaw);
        var newExpiry = refreshToken.IsRememberMe ? DateTime.UtcNow.AddDays(30) : DateTime.UtcNow.AddDays(1);

        refreshToken.RevokedAt = DateTime.UtcNow;
        refreshToken.ReplacedByTokenHash = newRefreshHash;

        db.RefreshTokens.Add(new RefreshToken
        {
            TokenId = Guid.NewGuid(),
            UserId = refreshToken.UserId,
            TokenHash = newRefreshHash,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = newExpiry,
            IsRememberMe = refreshToken.IsRememberMe
        });

        await db.SaveChangesAsync(ct);
        WriteAuthCookies(accessToken, accessExpiry, newRefreshRaw, newExpiry);

        var response = new AuthResponse
        {
            UserId = refreshToken.User.UserId,
            Username = refreshToken.User.Username,
            Email = refreshToken.User.Email,
            Role = refreshToken.User.Role.ToString(),
            Token = accessToken,
            ExpiresAtUtc = accessExpiry
        };

        return Ok(ApiResponse<AuthResponse>.Ok(response, "Session refreshed."));
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me(CancellationToken ct)
    {
        var userId = User.GetUserId();
        var user = await userRepository.GetByIdAsync(userId, ct);
        if (user is null)
        {
            return Unauthorized(ApiResponse<object>.Fail("User not found."));
        }

        var (token, expiresAt) = jwtTokenService.Generate(user);
        WriteAccessTokenCookie(token, expiresAt);

        var response = new AuthResponse
        {
            UserId = user.UserId,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role.ToString(),
            Token = token,
            ExpiresAtUtc = expiresAt
        };

        return Ok(ApiResponse<AuthResponse>.Ok(response));
    }

    [HttpPost("logout")]
    [AllowAnonymous]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        if (Request.Cookies.TryGetValue(RefreshTokenCookieName, out var refreshTokenRaw) && !string.IsNullOrWhiteSpace(refreshTokenRaw))
        {
            var tokenHash = ComputeSha256(refreshTokenRaw);
            var refreshToken = await db.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == tokenHash, ct);
            if (refreshToken is not null && refreshToken.RevokedAt is null)
            {
                refreshToken.RevokedAt = DateTime.UtcNow;
                await db.SaveChangesAsync(ct);
            }
        }

        Response.Cookies.Delete(AccessTokenCookieName);
        Response.Cookies.Delete(RefreshTokenCookieName);
        return Ok(ApiResponse<object>.Ok(null, "Logged out."));
    }

    private void WriteAuthCookies(string accessToken, DateTime accessExpiry, string refreshToken, DateTime refreshExpiry)
    {
        WriteAccessTokenCookie(accessToken, accessExpiry);

        Response.Cookies.Append(RefreshTokenCookieName, refreshToken, new CookieOptions
        {
            HttpOnly = true,
            IsEssential = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = refreshExpiry,
            Path = "/"
        });
    }

    private void WriteAccessTokenCookie(string accessToken, DateTime accessExpiry)
    {
        Response.Cookies.Append(AccessTokenCookieName, accessToken, new CookieOptions
        {
            HttpOnly = true,
            IsEssential = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = accessExpiry,
            Path = "/"
        });
    }

    private static string GenerateRawToken() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

    private static string ComputeSha256(string value)
    {
        var bytes = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
