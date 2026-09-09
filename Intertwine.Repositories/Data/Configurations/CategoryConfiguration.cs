using Intertwine.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Intertwine.Repositories.Data.Configurations
{
    public class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.HasKey(c => c.CategoryId);

            builder.Property(c => c.CategoryName)
                .IsRequired()
                .HasMaxLength(200);

            builder.HasMany(c => c.QuestionCategories)
                .WithOne(qc => qc.Category)
                .HasForeignKey(qc => qc.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
