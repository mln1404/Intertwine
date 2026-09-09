namespace Intertwine.Domain.Entities
{
    /// <summary>
    /// Join entity linking a <see cref="Question"/> to a <see cref="Category"/>.
    /// </summary>
    public class QuestionCategories
    {
        public QuestionCategories()
        {
            Category = new();
            Question = new();
        }
        public int QuestionCategoryId { get; set; }
        public int QuestionId { get; set; }
        public int CategoryId { get; set; }

        /// <summary>
        /// The category side of the relationship.
        /// </summary>
        public Category Category { get; set; }

        /// <summary>
        /// The question side of the relationship.
        /// </summary>
        public Question Question { get; set; }
    }
}
