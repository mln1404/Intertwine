using Intertwine.Domain.Abstractions;

namespace Intertwine.Domain.Entities
{
    /// <summary>
    /// Represents the question selected for a particular day.
    /// </summary>
    public class DailyQuestion : BaseEntity
    {
        public int DailyQuestionId { get; set; }

        /// <summary>
        /// The date this entry applies to (date component only).
        /// </summary>
        public DateOnly Date { get; set; }

        /// <summary>
        /// FK to the related <see cref="Question"/>.
        /// </summary>
        public int QuestionId { get; set; }

        /// <summary>
        /// The <see cref="Question"/> chosen for the day.
        /// </summary>
        public Question Question { get; set; } = null!;

    }
}
