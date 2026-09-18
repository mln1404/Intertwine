using Intertwine.Domain.Entities;
using Intertwine.Identity;
using Intertwine.Services.Constants;
using Intertwine.Services.DTOs.Authentication;
using Intertwine.Services.Interfaces;
using Intertwine.Services.Interfaces.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Intertwine.Services.Services;

/// <summary>
/// Handles registration, login, refresh-token rotation, and logout for identity accounts.
/// </summary>
public class AuthService : IAuthService
{
    private readonly JwtSettings _jwtSettings;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserProfileRepository _userProfileRepository;
    private readonly ITokenService _tokenService;

    public AuthService(
    UserManager<ApplicationUser> userManager,
    IUserProfileRepository userProfileRepository,
    ITokenService tokenService,
    IRefreshTokenRepository refreshTokenRepository,
    IUnitOfWork unitOfWork,
    IOptions<JwtSettings> jwtOptions)
    {
        _userManager = userManager;
        _userProfileRepository = userProfileRepository;
        _tokenService = tokenService;
        _refreshTokenRepository = refreshTokenRepository;
        _unitOfWork = unitOfWork;
        _jwtSettings = jwtOptions.Value;
    }

    /// <inheritdoc />
    public async Task<AuthResult> LoginAsync(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

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

        var accessToken = _tokenService.GenerateAccessToken(user.Id, user.Email!);

        var refreshToken = _tokenService.GenerateRefreshToken();
        var refreshTokenExpiresAtUtc = DateTime.UtcNow.AddDays(
            _jwtSettings.RefreshTokenExpiryDays);
        var refreshTokenEntity = new RefreshToken
        {
            IdentityUserId = user.Id,
            TokenHash = _tokenService.HashRefreshToken(refreshToken),
            CreatedAtUtc = DateTime.UtcNow,
            ExpiresAtUtc = refreshTokenExpiresAtUtc
        };

        await _refreshTokenRepository.AddAsync(refreshTokenEntity);

        await _unitOfWork.SaveChangesAsync();

        return new AuthResult
        {
            Succeeded = true,
            UserId = user.Id,
            Token = accessToken,
            RefreshToken = refreshToken,
            RefreshTokenExpiresAtUtc = refreshTokenExpiresAtUtc
        };
    }
    /// <inheritdoc />
    public async Task<AuthResult> RefreshAsync(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return new AuthResult
            {
                Succeeded = false,
                Error = AuthMessages.InvalidRefreshToken
            };
        }

        var tokenHash = _tokenService.HashRefreshToken(refreshToken);

        var existingToken = await _refreshTokenRepository.GetByHashAsync(tokenHash);

        if (existingToken == null ||
            existingToken.RevokedAtUtc != null ||
            existingToken.ExpiresAtUtc <= DateTime.UtcNow)
        {
            return new AuthResult
            {
                Succeeded = false,
                Error = AuthMessages.InvalidRefreshToken
            };
        }

        var now = DateTime.UtcNow;

        // Generate the replacement refresh token.
        var newRefreshToken = _tokenService.GenerateRefreshToken();
        var newRefreshTokenHash = _tokenService.HashRefreshToken(newRefreshToken);

        // Sliding expiration: create a new refresh token and revoke the old one.
        var newRefreshTokenEntity = new RefreshToken
        {
            IdentityUserId = existingToken.IdentityUserId,
            TokenHash = newRefreshTokenHash,
            CreatedAtUtc = now,
            ExpiresAtUtc = now.AddDays(_jwtSettings.RefreshTokenExpiryDays)
        };

        // Revoke the token that was just used.
        existingToken.RevokedAtUtc = now;
        existingToken.ReplacedByTokenHash = newRefreshTokenHash;

        await _refreshTokenRepository.AddAsync(newRefreshTokenEntity);

        await _unitOfWork.SaveChangesAsync();

        var accessToken =
            _tokenService.GenerateAccessToken(
                existingToken.User.Id,
                existingToken.User.Email!);

        return new AuthResult
        {
            Succeeded = true,
            UserId = existingToken.User.Id,
            Token = accessToken,

            // JsonIgnore properties
            RefreshToken = newRefreshToken,
            RefreshTokenExpiresAtUtc =
                newRefreshTokenEntity.ExpiresAtUtc
        };
    }

    /// <inheritdoc />
    public async Task LogoutAsync(string? refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            return;

        var tokenHash = _tokenService.HashRefreshToken(refreshToken);
        var existingToken = await _refreshTokenRepository.GetByHashAsync(tokenHash);
        if (existingToken == null || existingToken.RevokedAtUtc != null)
            return;

        existingToken.RevokedAtUtc = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();
    }

    /// <inheritdoc />
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

}
