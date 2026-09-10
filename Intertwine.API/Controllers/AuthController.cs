using Intertwine.Services.DTOs.Authentication;
using Intertwine.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Intertwine.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);

        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);

        if (!result.Succeeded)
        {
            return Unauthorized(result);
        }

        return Ok(result);
    }
}

