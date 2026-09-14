using Intertwine.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Intertwine.Repositories.Data.Configurations
{
    /// <summary>
    /// Configures the database mapping for <see cref="Answer"/>.
    /// </summary>
    public class AnswerConfiguration : IEntityTypeConfiguration<Answer>
    {
        public void Configure(EntityTypeBuilder<Answer> builder)
        {
            builder.HasKey(a => a.AnswerId);

            builder.HasIndex(x => x.QuestionId);

            builder.Property(a => a.AnswerText)
                .IsRequired()
                .HasMaxLength(1000);

            builder.HasOne(d => d.Question)
                .WithMany(q => q.Answers)
                .HasForeignKey(d => d.QuestionId)
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
