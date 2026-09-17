using Intertwine.Domain.Entities;
using Intertwine.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Intertwine.Repositories.Data
{
    /// <summary>
    /// EF Core database context for Intertwine's identity and application data.
    /// </summary>
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

        /// <summary>
        /// The users' daily activity records, tracking their interactions with the application.
        /// </summary>
        public DbSet<UserDailyActivity> UserDailyActivities => Set<UserDailyActivity>();


        #region Financial
        public DbSet<UserWallet> UserWallets => Set<UserWallet>();
        public DbSet<Currency> Currencies => Set<Currency>();
        public DbSet<CreditPackage> CreditPackages => Set<CreditPackage>();
        public DbSet<UserPayment> UserPayments => Set<UserPayment>();
        public DbSet<FinancialTransaction> FinancialTransactions
            => Set<FinancialTransaction>();
        #endregion

        #region Authentication
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        #endregion

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(
                typeof(IntertwineDbContext).Assembly);
        }
    }
}
