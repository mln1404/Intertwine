using Microsoft.AspNetCore.Identity;
using Intertwine.Domain.Entities;

namespace Intertwine.Identity;

public class ApplicationUser : IdentityUser
{
    public UserProfile UserProfile { get; set; } = null!;

    public ICollection<RefreshToken> RefreshTokens { get; set; }
        = new List<RefreshToken>();
}
