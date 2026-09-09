namespace Intertwine.Domain.Entities
{
    /// <summary>
    /// A category used to group <see cref="Question"/> entities.
    /// </summary>
    public class Category
    {
        public Category()
        {
            QuestionCategories = new List<QuestionCategories>();
        }

        public int CategoryId { get; set; }
        /// <summary>
        /// Category display name.
        /// </summary>
        public string CategoryName { get; set; } = string.Empty;

        /// <summary>
        /// The join collection linking this category to <see cref="Question"/> entities.
        /// </summary>
        public ICollection<QuestionCategories> QuestionCategories { get; set; }
    }
}
