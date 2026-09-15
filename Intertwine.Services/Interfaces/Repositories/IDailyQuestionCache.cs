using Intertwine.Services.DTOs.Questions;

namespace Intertwine.Services.Interfaces.Repositories;

/// <summary>
/// Provides shared caching for Daily Question response data.
/// </summary>
public interface IDailyQuestionCache
{
    /// <summary>
    /// Gets the cached Daily Question for a local date, if one exists.
    /// </summary>
    Task<QuestionDetailDto?> GetAsync(
        DateOnly localDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Stores a Daily Question response for its local date.
    /// </summary>
    Task SetAsync(
        DateOnly localDate,
        QuestionDetailDto question,
        CancellationToken cancellationToken = default);
}
