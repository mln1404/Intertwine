using Intertwine.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Intertwine.Repositories.Data.Configurations
{
    public class AnswerConfiguration : IEntityTypeConfiguration<Answer>
    {
        public void Configure(EntityTypeBuilder<Answer> builder)
        {
            builder.HasKey(a => a.AnswerId);

            builder.Property(a => a.AnswerText)
                .IsRequired()
                .HasMaxLength(1000);

            // Answer -> Question (QuestionId is a shadow FK, Question does not expose QuestionId)
            builder.HasOne(a => a.Question)
                .WithMany()
                .HasForeignKey("QuestionId")
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
