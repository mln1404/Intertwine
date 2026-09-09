using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Intertwine.API.Models;
using Intertwine.API.Services;

namespace Intertwine.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ITokenService _tokenService;
    private readonly JwtSettings _jwtSettings;

    public AuthController(ITokenService tokenService, IOptions<JwtSettings> options)
    {
        _tokenService = tokenService;
        _jwtSettings = options.Value;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest("Username and password are required.");
        }

        // TODO: Replace this in-memory check with a real user store.
        // Simple demo user: username 'admin' and password 'Password123!'
        if (!ValidateCredentials(request.Username, request.Password, out var userId))
        {
            return Unauthorized();
        }

        var token = _tokenService.GenerateToken(userId, request.Username);
        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes);

        var response = new LoginResponse
        {
            Token = token,
            UserName = request.Username,
            ExpiresAtUtc = expiresAt
        };

        return Ok(response);
    }

    private bool ValidateCredentials(string username, string password, out string userId)
    {
        userId = string.Empty;
        // Demo-only: a single hard-coded user
        if (username == "admin" && password == "Password123!")
        {
            userId = "1";
            return true;
        }

        return false;
    }
}
