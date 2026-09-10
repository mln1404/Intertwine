using Intertwine.Domain.Entities;
using Intertwine.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Intertwine.Repositories.Data
{
    public class IntertwineDbContext : IdentityDbContext<ApplicationUser>
    {
        public IntertwineDbContext(
            DbContextOptions<IntertwineDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Users/Profiles in the system.
        /// </summary>
        public DbSet<UserProfile> UserProfiles => Set<UserProfile>();

        /// <summary>
        /// Questions managed by the application.
        /// </summary>
        public DbSet<Question> Questions => Set<Question>();

        /// <summary>
        /// Multiple choice answers for questions.
        /// </summary>
        public DbSet<Answer> Answers => Set<Answer>();

        /// <summary>
        /// Categories that group questions.
        /// </summary>
        public DbSet<Category> Categories => Set<Category>();

        /// <summary>
        /// Join table linking questions and categories.
        /// </summary>
        public DbSet<QuestionCategories> QuestionCategories => Set<QuestionCategories>();

        /// <summary>
        /// Daily question selections.
        /// </summary>
        public DbSet<DailyQuestion> DailyQuestions => Set<DailyQuestion>();

        /// <summary>
        /// Answers provided by users.
        /// </summary>
        public DbSet<UserAnswers> UserAnswers => Set<UserAnswers>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(
                typeof(IntertwineDbContext).Assembly);
        }
    }
}
