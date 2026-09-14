using Intertwine.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Intertwine.Repositories.Data.Configurations
{
    /// <summary>
    /// Configures the database mapping for <see cref="UserAnswers"/>.
    /// </summary>
    public class UserAnswersConfiguration : IEntityTypeConfiguration<UserAnswers>
    {
        public void Configure(EntityTypeBuilder<UserAnswers> builder)
        {
            builder.HasKey(ua => ua.UserAnswerId);

            builder
                .HasIndex(x => new
                {
                    x.UserProfileId,
                    x.AnswerId
                });

            builder.HasOne(ua => ua.Answer)
                .WithMany()
                .HasForeignKey(ua => ua.AnswerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(ua => ua.UserProfile)
                .WithMany(up => up.UserAnswers)
                .HasForeignKey(ua => ua.UserProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(x => x.DateCreated)
                .HasDefaultValueSql(SqlServerDefaults.UtcDateTime);

            builder.Property(x => x.DateUpdated)
                .HasDefaultValueSql(SqlServerDefaults.UtcDateTime);
        }
    }
}
