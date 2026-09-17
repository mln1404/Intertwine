namespace Intertwine.Identity;

public class RefreshToken
{
    public long RefreshTokenId { get; set; }

    public string IdentityUserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;

    public string TokenHash { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }

    public DateTime ExpiresAtUtc { get; set; }

    public DateTime? RevokedAtUtc { get; set; }

    public string? ReplacedByTokenHash { get; set; }
}
