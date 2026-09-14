using Intertwine.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Intertwine.Repositories.Data.Configurations
{
    /// <summary>
    /// Configures the database mapping for <see cref="Question"/>.
    /// </summary>
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

            builder.HasMany(q => q.Answers)
                .WithOne(a => a.Question)
                .HasForeignKey(a => a.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(x => x.DateCreated)
                .HasDefaultValueSql(SqlServerDefaults.UtcDateTime);

            builder.Property(x => x.DateUpdated)
                .HasDefaultValueSql(SqlServerDefaults.UtcDateTime);

            builder.Property(x => x.IsActive)
                .HasDefaultValue(true);
        }
    }
}
