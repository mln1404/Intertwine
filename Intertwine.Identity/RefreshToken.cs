namespace Intertwine.Identity;

/// <summary>
/// Stores the server-side state of one refresh token without persisting the raw token value.
/// </summary>
public class RefreshToken
{
    /// <summary>Gets or sets the database identifier.</summary>
    public long RefreshTokenId { get; set; }

    /// <summary>Gets or sets the owning ASP.NET Identity user identifier.</summary>
    public string IdentityUserId { get; set; } = string.Empty;

    /// <summary>Gets or sets the owning identity account.</summary>
    public ApplicationUser User { get; set; } = null!;

    /// <summary>Gets or sets the SHA-256 hash of the raw refresh token.</summary>
    public string TokenHash { get; set; } = string.Empty;

    /// <summary>Gets or sets when this token record was created in UTC.</summary>
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>Gets or sets when this token expires in UTC.</summary>
    public DateTime ExpiresAtUtc { get; set; }

    /// <summary>Gets or sets when the token was revoked in UTC.</summary>
    public DateTime? RevokedAtUtc { get; set; }

    /// <summary>Gets or sets the hash of the replacement token created by rotation.</summary>
    public string? ReplacedByTokenHash { get; set; }
}
