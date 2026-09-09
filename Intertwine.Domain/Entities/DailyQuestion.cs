namespace Intertwine.Domain.Entities
{
    /// <summary>
    /// Represents the question selected for a particular day.
    /// </summary>
    public class DailyQuestion
    {
        public DailyQuestion()
        {
            Question = new();
        }

        public int DailyQuestionId { get; set; }
        public DateTime DateNow { get; set; }
        public int QuestionId { get; set; }
        /// <summary>
        /// The <see cref="Question"/> chosen for the day.
        /// </summary>
        public Question Question { get; set; }

    }
}
