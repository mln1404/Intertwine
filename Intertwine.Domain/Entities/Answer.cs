namespace Intertwine.Domain.Entities
{
    /// <summary>
    /// Multiple choice answer for a <see cref="Question"/>.
    /// </summary>
    /// <remarks>
    /// Navigation property <see cref="Question"/> links the answer to its parent question.
    /// </remarks>
    public class Answer
    {
        public Answer()
        {
            Question = new();
        }

        public int AnswerId { get; set; }
        /// <summary>
        /// The text of the answer option.
        /// </summary>
        public string AnswerText { get; set; } = string.Empty;

        /// <summary>
        /// The <see cref="Question"/> this answer belongs to.
        /// </summary>
        public Question Question { get; set; }
    }
}
