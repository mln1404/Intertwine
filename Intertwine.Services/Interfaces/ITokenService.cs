namespace Intertwine.Services.Interfaces;

/// <summary>
/// Creates and hashes authentication tokens.
/// </summary>
public interface ITokenService
{
    /// <summary>Creates a signed JWT access token for an authenticated user.</summary>
    string GenerateAccessToken(string userId, string userName);

    /// <summary>Creates an opaque token suitable for an HttpOnly refresh-token cookie.</summary>
    string GenerateRefreshToken();

    /// <summary>Produces the one-way hash persisted for a refresh token.</summary>
    string HashRefreshToken(string refreshToken);
}
