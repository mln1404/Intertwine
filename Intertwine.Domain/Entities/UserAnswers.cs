namespace Intertwine.Domain.Entities
{
    /// <summary>
    /// Represents an answer provided by a <see cref="UserProfile"/> to an <see cref="Answer"/>.
    /// </summary>
    public class UserAnswers : BaseEntity
    {
        public UserAnswers()
        {
        }

        public int UserAnswerId { get; set; }

        public int UserProfileId { get; set; }

        public int AnswerId { get; set; }

        /// <summary>
        /// The selected <see cref="Answer"/>.
        /// </summary>
        public Answer Answer { get; set; } = null!;

        /// <summary>
        /// The <see cref="UserProfile"/> who selected the answer.
        /// </summary>
        public UserProfile UserProfile { get; set; } = null!;
    }
}
