using Intertwine.Identity;
using Intertwine.Services.DTOs.Authentication;
using Intertwine.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Intertwine.Services.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserProfileRepository _userProfileRepository;
    private readonly ITokenService _tokenService;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        IUserProfileRepository userProfileRepository,
        ITokenService tokenService)
    {
        _userManager = userManager;
        _userProfileRepository = userProfileRepository;
        _tokenService = tokenService;
    }

    public async Task<AuthResult> RegisterAsync(
        RegisterRequest request)
    {
        var existingUser = await _userManager
            .FindByEmailAsync(request.Email);

        if (existingUser != null)
        {
            return new AuthResult
            {
                Succeeded = false,
                Error = "An account with this email already exists."
            };
        }

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email
        };

        var result = await _userManager.CreateAsync(
            user,
            request.Password);

        if (!result.Succeeded)
        {
            return new AuthResult
            {
                Succeeded = false,
                Error = string.Join(
                    "; ",
                    result.Errors.Select(x => x.Description))
            };
        }

        return new AuthResult
        {
            Succeeded = true,
            UserId = user.Id
        };
    }

    public async Task<AuthResult> LoginAsync(
        LoginRequest request)
    {
        var user = await _userManager
            .FindByEmailAsync(request.Email);

        if (user == null)
        {
            return new AuthResult
            {
                Succeeded = false,
                Error = "Invalid email or password."
            };
        }

        var validPassword = await _userManager
            .CheckPasswordAsync(user, request.Password);

        if (!validPassword)
        {
            return new AuthResult
            {
                Succeeded = false,
                Error = "Invalid email or password."
            };
        }

        return new AuthResult
        {
            Succeeded = true,
            UserId = user.Id
        };
    }
}