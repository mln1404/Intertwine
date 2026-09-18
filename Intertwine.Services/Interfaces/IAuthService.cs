using Intertwine.Services.DTOs.Authentication;

namespace Intertwine.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResult> LoginAsync(LoginRequest request);
    Task<AuthResult> RefreshAsync(string refreshToken);
    Task LogoutAsync(string? refreshToken);
    Task<AuthResult> RegisterAsync(RegisterRequest request);
}
