using System.Text.Json.Serialization;

namespace Intertwine.Services.DTOs.Authentication;

public class AuthResult
{
    public bool Succeeded { get; set; }
    public string? Error { get; set; }
    public string? Token { get; set; }
    public string? UserId { get; set; }

    [JsonIgnore]
    public string? RefreshToken { get; set; }

    [JsonIgnore]
    public DateTime? RefreshTokenExpiresAtUtc{ get; set; }
}
