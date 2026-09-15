using Intertwine.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Intertwine.Repositories.Data.Configurations;

public class UserPaymentConfiguration
    : IEntityTypeConfiguration<UserPayment>
{
    public void Configure(EntityTypeBuilder<UserPayment> builder)
    {
        builder.HasKey(x => x.UserPaymentId);

        builder
            .Property(x => x.Amount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder
            .Property(x => x.CreditsPurchased)
            .IsRequired();

        builder
            .Property(x => x.PaymentProvider)
            .HasMaxLength(50)
            .IsRequired();

        builder
            .Property(x => x.ProviderTransactionId)
            .HasMaxLength(200);

        builder
            .Property(x => x.Status)
            .HasConversion<int>()
            .IsRequired();

        builder
            .Property(x => x.RowVersion)
            .IsRowVersion();

        builder
            .HasOne(x => x.UserProfile)
            .WithMany()
            .HasForeignKey(x => x.UserProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.Currency)
            .WithMany()
            .HasForeignKey(x => x.CurrencyCode)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasIndex(x => new
            {
                x.UserProfileId,
                x.DateCreated
            });

        builder
            .HasIndex(x => x.ProviderTransactionId);
    }
}
