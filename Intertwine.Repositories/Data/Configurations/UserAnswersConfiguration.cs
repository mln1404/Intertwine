using Intertwine.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Intertwine.Repositories.Data.Configurations
{
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
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(x => x.DateUpdated)
                .HasDefaultValueSql("GETUTCDATE()");
        }
    }
}
