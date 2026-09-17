namespace Intertwine.Services.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(string userId, string userName);

    string GenerateRefreshToken();

    string HashRefreshToken(string refreshToken);
}
