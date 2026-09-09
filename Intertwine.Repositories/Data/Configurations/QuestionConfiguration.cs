using Intertwine.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Intertwine.Repositories.Data.Configurations
{
    public class QuestionConfiguration : IEntityTypeConfiguration<Question>
    {
        public void Configure(EntityTypeBuilder<Question> builder)
        {
            builder.HasKey(q => q.QuestionId);

            builder.Property(q => q.QuestionTitle)
                .IsRequired()
                .HasMaxLength(300);

            builder.Property(q => q.FullQuestion)
                .IsRequired()
                .HasMaxLength(4000);

            builder.HasMany(q => q.QuestionCategories)
                .WithOne(qc => qc.Question)
                .HasForeignKey(qc => qc.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(q => q.DailyQuestions)
                .WithOne(dq => dq.Question)
                .HasForeignKey(dq => dq.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
