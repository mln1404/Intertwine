using Intertwine.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Intertwine.Repositories.Data.Configurations;

/// <summary>
/// Configures the one-wallet-per-profile rule, optimistic concurrency, and non-negative balance.
/// </summary>
public class UserWalletConfiguration
    : IEntityTypeConfiguration<UserWallet>
{
    public void Configure(EntityTypeBuilder<UserWallet> builder)
    {
        builder.HasKey(x => x.UserWalletId);

        builder
            .Property(x => x.CreditBalance)
            .HasDefaultValue(0L);

        builder
            .Property(x => x.RowVersion)
            .IsRowVersion();

        builder
            .HasIndex(x => x.UserProfileId)
            .IsUnique();

        builder.ToTable(t =>
        {
            t.HasCheckConstraint(
                "CK_UserWallets_CreditBalance_NonNegative",
                "[CreditBalance] >= 0");
        });
    }
}
