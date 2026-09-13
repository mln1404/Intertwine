using Intertwine.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Intertwine.Repositories.Data.Configurations
{
    public class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
    {
        public void Configure(EntityTypeBuilder<UserProfile> builder)
        {
            builder.HasKey(up => up.UserProfileId);

            builder.Property(up => up.AvatarName)
                .HasMaxLength(200);

            builder.Property(up => up.FirstName)
                .HasMaxLength(200);

            builder.Property(up => up.LastName)
                .HasMaxLength(200);

            builder.Property(up => up.MiddleName)
                .HasMaxLength(200);

            builder.HasMany(up => up.UserAnswers)
                .WithOne(ua => ua.UserProfile)
                .HasForeignKey(ua => ua.UserProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(x => x.DateCreated)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(x => x.DateUpdated)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(x => x.IsActive)
                .HasDefaultValue(true);
        }
    }
}
