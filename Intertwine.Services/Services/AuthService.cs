using Intertwine.Domain.Entities;
using Intertwine.Identity;
using Intertwine.Services.Constants;
using Intertwine.Services.DTOs.Authentication;
using Intertwine.Services.Interfaces;
using Intertwine.Services.Interfaces.Repositories;
using Microsoft.AspNetCore.Identity;

namespace Intertwine.Services.Services;

/// <summary>
/// Handles identity registration and credential validation.
/// </summary>
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
                Error = AuthMessages.DuplicateEmail
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
                    AuthMessages.ErrorDelimiter,
                    result.Errors.Select(x => x.Description))
            };
        }

        var userProfile = new UserProfile
        {
            IdentityUserId = user.Id,
            FirstName = request.FirstName,
            LastName = request.LastName,
            DateCreated = DateTime.UtcNow,
            CreatedBy = user.Id,
            UserWallet = new UserWallet
            {
                CreditBalance = 0,
                DateCreated = DateTime.UtcNow,
                CreatedBy = user.Id
            }
        };

        await _userProfileRepository.AddAsync(userProfile);

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
                Error = AuthMessages.InvalidCredentials
            };
        }

        var validPassword = await _userManager
            .CheckPasswordAsync(user, request.Password);

        if (!validPassword)
        {
            return new AuthResult
            {
                Succeeded = false,
                Error = AuthMessages.InvalidCredentials
            };
        }

        var userProfile = await _userProfileRepository
            .GetByIdentityUserIdIncludingInactiveAsync(user.Id);
        if (userProfile is { IsActive: false })
        {
            await _userProfileRepository.SetIsActiveAsync(
                userProfile,
                true,
                user.Id);
        }

        var token = _tokenService.GenerateAccessToken(user.Id, user.Email!);
        return new AuthResult
        {
            Succeeded = true,
            UserId = user.Id,
            Token = token
        };
    }
}
