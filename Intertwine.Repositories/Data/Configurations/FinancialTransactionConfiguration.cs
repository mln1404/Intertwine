using Intertwine.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Intertwine.Repositories.Data.Configurations;

/// <summary>
/// Configures wallet ledger entry persistence and its relationships.
/// </summary>
public class FinancialTransactionConfiguration
    : IEntityTypeConfiguration<FinancialTransaction>
{
    public void Configure(
        EntityTypeBuilder<FinancialTransaction> builder)
    {
        builder.HasKey(x => x.FinancialTransactionId);

        builder
            .Property(x => x.CreditAmount)
            .IsRequired();

        builder
            .Property(x => x.BalanceAfterTransaction)
            .IsRequired();

        builder
            .Property(x => x.TransactionType)
            .HasConversion<int>()
            .IsRequired();

        builder
            .Property(x => x.Description)
            .HasMaxLength(500);

        builder
            .HasOne(x => x.UserWallet)
            .WithMany()
            .HasForeignKey(x => x.UserWalletId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.UserPayment)
            .WithMany()
            .HasForeignKey(x => x.UserPaymentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasIndex(x => new
            {
                x.UserWalletId,
                x.DateCreated
            });

        builder
            .HasIndex(x => x.UserPaymentId);
    }
}
