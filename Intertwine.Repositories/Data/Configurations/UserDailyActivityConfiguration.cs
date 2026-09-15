using Intertwine.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Intertwine.Repositories.Data.Configurations;

/// <summary>
/// Configures the database mapping and daily uniqueness constraint for <see cref="UserDailyActivity"/>.
/// </summary>
public class UserDailyActivityConfiguration
    : IEntityTypeConfiguration<UserDailyActivity>
{
    public void Configure(
        EntityTypeBuilder<UserDailyActivity> builder)
    {
        builder
            .HasIndex(x => new
            {
                x.UserProfileId,
                x.Date
            })
            .IsUnique();

        builder
            .Property(x => x.NonDailyQuestionsAnswered)
            .IsConcurrencyToken()
            .HasDefaultValue(0);

        builder
            .Property(x => x.DailyQuestionCreateOrUpdateUsed)
            .IsConcurrencyToken()
            .HasDefaultValue(false);

        builder
            .HasOne(x => x.UserProfile)
            .WithMany()
            .HasForeignKey(x => x.UserProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
