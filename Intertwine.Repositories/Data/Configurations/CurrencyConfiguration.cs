using Intertwine.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Intertwine.Repositories.Data.Configurations;

/// <summary>
/// Configures persisted currencies used to price credit packages.
/// </summary>
public class CurrencyConfiguration
    : IEntityTypeConfiguration<Currency>
{
    public void Configure(EntityTypeBuilder<Currency> builder)
    {
        builder.HasKey(x => x.Code);

        builder
            .Property(x => x.Code)
            .HasMaxLength(3)
            .IsUnicode(false)
            .ValueGeneratedNever();

        builder
            .Property(x => x.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder
            .Property(x => x.Symbol)
            .HasMaxLength(10)
            .IsRequired();

        builder
            .Property(x => x.DecimalPlaces)
            .HasDefaultValue(2);
    }
}
