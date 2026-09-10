using Intertwine.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Intertwine.Repositories.Data.Configurations
{
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
        }
    }
}
