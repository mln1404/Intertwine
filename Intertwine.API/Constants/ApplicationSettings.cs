namespace Intertwine.API.Constants;

/// <summary>
/// Names used to retrieve application configuration and registered policies.
/// </summary>
public static class ApplicationSettings
{
    public const string DefaultConnection = "DefaultConnection";
    public const string RedisConnection = "RedisConnection";
    public const string JwtSettings = "JwtSettings";
    public const string LoginRateLimitPolicy = "login";
}
