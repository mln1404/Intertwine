using Intertwine.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Intertwine.Repositories.Data.Configurations;

public class CreditPackageConfiguration
    : IEntityTypeConfiguration<CreditPackage>
{
    public void Configure(EntityTypeBuilder<CreditPackage> builder)
    {
        builder.HasKey(x => x.CreditPackageId);

        builder
            .Property(x => x.Amount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder
            .Property(x => x.Credits)
            .IsRequired();

        builder
            .HasOne(x => x.Currency)
            .WithMany()
            .HasForeignKey(x => x.CurrencyCode)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasIndex(x => new
            {
                x.CurrencyCode,
                x.Amount
            });

        builder.ToTable(t =>
        {
            t.HasCheckConstraint(
                "CK_CreditPackages_Amount_Positive",
                "[Amount] > 0");

            t.HasCheckConstraint(
                "CK_CreditPackages_Credits_Positive",
                "[Credits] > 0");
        });
    }
}