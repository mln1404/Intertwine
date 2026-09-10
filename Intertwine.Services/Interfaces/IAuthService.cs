using Intertwine.Services.DTOs.Authentication;

namespace Intertwine.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResult> RegisterAsync(RegisterRequest request);
    Task<AuthResult> LoginAsync(LoginRequest request);
}