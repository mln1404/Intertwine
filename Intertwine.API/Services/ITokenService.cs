namespace Intertwine.API.Services;

public interface ITokenService
{
    string GenerateToken(string userId, string userName);
}
