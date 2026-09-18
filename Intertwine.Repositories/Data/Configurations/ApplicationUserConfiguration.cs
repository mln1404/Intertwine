using Intertwine.Domain.Entities;
using Intertwine.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Intertwine.Repositories.Data.Configurations;

/// <summary>
/// Configures the one-to-one relationship between an identity account and profile.
/// </summary>
public class ApplicationUserConfiguration
    : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder
            .HasOne(x => x.UserProfile)
            .WithOne()
            .HasForeignKey<UserProfile>(x => x.IdentityUserId)
            .IsRequired();
    }
}
