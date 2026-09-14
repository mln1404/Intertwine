using Intertwine.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Intertwine.Repositories.Data.Configurations
{
    /// <summary>
    /// Configures the database mapping for <see cref="DailyQuestion"/>.
    /// </summary>
    public class DailyQuestionConfiguration : IEntityTypeConfiguration<DailyQuestion>
    {
        public void Configure(EntityTypeBuilder<DailyQuestion> builder)
        {
            builder.HasKey(d => d.DailyQuestionId);

            builder.Property(d => d.Date)
                .IsRequired();

            builder.HasOne(d => d.Question)
                .WithMany(q => q.DailyQuestions)
                .HasForeignKey(d => d.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(x => x.DateCreated)
                .HasDefaultValueSql(SqlServerDefaults.UtcDateTime);

            builder.Property(x => x.DateUpdated)
                .HasDefaultValueSql(SqlServerDefaults.UtcDateTime);
        }
    }
}
