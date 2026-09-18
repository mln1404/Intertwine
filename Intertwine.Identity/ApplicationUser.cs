using Microsoft.AspNetCore.Identity;
using Intertwine.Domain.Entities;

namespace Intertwine.Identity;

/// <summary>
/// The ASP.NET Identity account linked one-to-one with an Intertwine profile.
/// </summary>
public class ApplicationUser : IdentityUser
{
    /// <summary>Gets or sets the application profile owned by this identity account.</summary>
    public UserProfile UserProfile { get; set; } = null!;

    /// <summary>Gets refresh-token records issued to this identity account.</summary>
    public ICollection<RefreshToken> RefreshTokens { get; set; }
        = new List<RefreshToken>();
}
