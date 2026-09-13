using Intertwine.Domain.Abstractions;

namespace Intertwine.Domain.Entities
{
    /// <summary>
    /// A category used to group <see cref="Question"/> entities.
    /// </summary>
    public class Category : ActivatableEntity
    {
        public int CategoryId { get; set; }

        /// <summary>
        /// Category display name.
        /// </summary>
        public string CategoryName { get; set; } = string.Empty;

        /// <summary>
        /// Category color in hexadecimal format (e.g., #FF5733).
        /// </summary>
        public string Color { get; set; } = string.Empty;

        /// <summary>
        /// The join collection linking this category to <see cref="Question"/> entities.
        /// </summary>
        public ICollection<QuestionCategories> QuestionCategories { get; set; } = [];
    }
}
