using Microsoft.AspNetCore.Identity;
using Intertwine.Domain.Entities;

namespace Intertwine.Repositories.Identity;

public class ApplicationUser : IdentityUser
{
    public UserProfile UserProfile { get; set; } = null!;
}