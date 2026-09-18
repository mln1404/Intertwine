using Intertwine.Services.DTOs.Authentication;

namespace Intertwine.Services.Interfaces;

/// <summary>
/// Provides registration, credential validation, refresh-token rotation, and logout operations.
/// </summary>
public interface IAuthService
{
    /// <summary>Authenticates a user and creates an access-token and refresh-token session.</summary>
    Task<AuthResult> LoginAsync(LoginRequest request);

    /// <summary>Rotates a valid refresh token and issues a new access token.</summary>
    Task<AuthResult> RefreshAsync(string refreshToken);

    /// <summary>Revokes the session represented by the supplied refresh token when it exists.</summary>
    Task LogoutAsync(string? refreshToken);

    /// <summary>Creates an identity account, profile, and initial wallet.</summary>
    Task<AuthResult> RegisterAsync(RegisterRequest request);
}
