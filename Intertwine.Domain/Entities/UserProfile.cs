using Intertwine.Domain.Abstractions;

namespace Intertwine.Domain.Entities
{
    /// <summary>
    /// Represents a user's profile within the system.
    /// </summary>
    public class UserProfile : ActivatableEntity
    {
        public UserProfile()
        {
            UserAnswers = new List<UserAnswers>();
        }

        public int UserProfileId { get; set; }
        /// <summary>
        /// Optional avatar identifier for the user.
        /// </summary>
        public string AvatarName { get; set; } = string.Empty;

        /// <summary>
        /// User's first name.
        /// </summary>
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// User's last name.
        /// </summary>
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// User's middle name.
        /// </summary>
        public string MiddleName { get; set; } = string.Empty;

        /// <summary>
        /// Answers provided by the user.
        /// </summary>
        public ICollection<UserAnswers> UserAnswers { get; set; }

        /// <summary>
        /// Wallet containing the user's current Intertwine credit balance.
        /// </summary>
        public UserWallet? UserWallet { get; set; }
        
        /// <summary>
        /// FK to ASP.NET Identity's UserId
        /// </summary>
        public string IdentityUserId { get; set; } = string.Empty;

    }
}
