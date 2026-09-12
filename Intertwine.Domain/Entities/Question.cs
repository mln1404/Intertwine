using Intertwine.Domain.Abstractions;

namespace Intertwine.Domain.Entities
{
    /// <summary>
    /// A question with its title and the full question text. Links to categories and daily selections.
    /// </summary>
    public class Question : ActivatableEntity
    {
        public int QuestionId { get; set; }
        /// <summary>
        /// Full text of the question.
        /// </summary>
        public string FullQuestion { get; set; } = string.Empty;

        /// <summary>
        /// A short title for the question.
        /// </summary>
        public string QuestionTitle { get; set; } = string.Empty;

        /// <summary>
        /// Categories linked to this question via <see cref="QuestionCategories"/>.
        /// </summary>
        public ICollection<QuestionCategories> QuestionCategories { get; set; } = [];

        /// <summary>
        /// Daily question entries referencing this question.
        /// </summary>
        public ICollection<DailyQuestion> DailyQuestions { get; set; } = [];
    }
}
