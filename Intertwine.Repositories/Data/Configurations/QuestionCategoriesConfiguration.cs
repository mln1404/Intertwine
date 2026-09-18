using Intertwine.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Intertwine.Repositories.Data.Configurations
{
    /// <summary>
    /// Configures the question-to-category join table.
    /// </summary>
    public class QuestionCategoriesConfiguration : IEntityTypeConfiguration<QuestionCategories>
    {
        public void Configure(EntityTypeBuilder<QuestionCategories> builder)
        {
            builder.HasKey(qc => qc.QuestionCategoryId);

            builder.HasOne(qc => qc.Category)
                .WithMany(c => c.QuestionCategories)
                .HasForeignKey(qc => qc.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(qc => qc.Question)
                .WithMany(q => q.QuestionCategories)
                .HasForeignKey(qc => qc.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
