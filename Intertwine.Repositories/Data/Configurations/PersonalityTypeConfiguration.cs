using Intertwine.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Intertwine.Repositories.Data.Configurations;

/// <summary>
/// Configures the database mapping for <see cref="PersonalityType"/>.
/// </summary>
public class PersonalityTypeConfiguration : IEntityTypeConfiguration<PersonalityType>
{
    public void Configure(EntityTypeBuilder<PersonalityType> builder)
    {
        builder.HasKey(x => x.PersonalityTypeId);

        builder.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(4)
            .IsUnicode(false);

        builder.Property(x => x.Name)
            .HasMaxLength(100);

        builder.Property(x => x.Description)
            .HasMaxLength(1000);

        builder.HasIndex(x => x.Code)
            .IsUnique();

        builder.HasMany(x => x.UserProfiles)
            .WithOne(x => x.PersonalityType)
            .HasForeignKey(x => x.PersonalityTypeId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Property(x => x.DateCreated)
            .HasDefaultValueSql(SqlServerDefaults.UtcDateTime);

        builder.Property(x => x.DateUpdated)
            .HasDefaultValueSql(SqlServerDefaults.UtcDateTime);

        builder.Property(x => x.IsActive)
            .HasDefaultValue(true);
    }
}
