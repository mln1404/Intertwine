using Intertwine.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Intertwine.Repositories.Data.Configurations;

/// <summary>
/// Configures hashed refresh-token persistence and lookup constraints.
/// </summary>
public class RefreshTokenConfiguration
    : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(
        EntityTypeBuilder<RefreshToken> builder)
    {
        builder.HasKey(x => x.RefreshTokenId);

        builder.Property(x => x.IdentityUserId)
            .IsRequired();

        builder.Property(x => x.TokenHash)
            .HasMaxLength(64)
            .IsRequired();

        builder.HasIndex(x => x.TokenHash)
            .IsUnique();

        builder.HasIndex(x => new
        {
            x.IdentityUserId,
            x.ExpiresAtUtc
        });

        builder
            .HasOne(x => x.User)
            .WithMany(x => x.RefreshTokens)
            .HasForeignKey(x => x.IdentityUserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
