using Intertwine.Services.DTOs.Authentication;
using Intertwine.API.Constants;
using Intertwine.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Intertwine.API.Controllers;

[EnableRateLimiting(ApplicationSettings.LoginRateLimitPolicy)]
[ApiController]
[Route("api/[controller]")]
/// <summary>
/// Exposes registration and sign-in endpoints.
/// </summary>
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    /// <summary>
    /// Authenticates a user and returns an access token.
    /// </summary>
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);

        if (!result.Succeeded)
        {
            return Unauthorized(result);
        }

        if (!string.IsNullOrWhiteSpace(result.RefreshToken))
        {
            Response.Cookies.Append(
                AuthConstants.RefreshTokenCookieName,
                result.RefreshToken,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = result.RefreshTokenExpiresAtUtc
                });
        }

        return Ok(result);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh()
    {
        if (!Request.Cookies.TryGetValue(
            AuthConstants.RefreshTokenCookieName,
            out var refreshToken))
        {
            return Unauthorized();
        }

        var result = await _authService.RefreshAsync(refreshToken);

        if (!result.Succeeded)
        {
            Response.Cookies.Delete(AuthConstants.RefreshTokenCookieName);
            return Unauthorized(result);
        }

        Response.Cookies.Append(
            AuthConstants.RefreshTokenCookieName,
            result.RefreshToken!,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = result.RefreshTokenExpiresAtUtc
            });

        return Ok(result);
    }

    [HttpPost("register")]
    /// <summary>
    /// Registers an identity user from the supplied credentials.
    /// </summary>
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);

        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}
