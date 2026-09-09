namespace Intertwine.API.Models;

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
}
