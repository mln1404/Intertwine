using Intertwine.Domain.Entities;

namespace Intertwine.Services.Interfaces.Repositories;

/// <summary>
/// Provides query operations over active questions and answers.
/// </summary>
public interface IQuestionRepository
{
    /// <summary>Gets every active question with its category data.</summary>
    Task<IEnumerable<Question>> GetAllActiveAsync(
        CancellationToken cancellationToken = default);

    /// <summary>Gets active questions assigned to one category.</summary>
    Task<IEnumerable<Question>> GetByCategoryAsync(
        int categoryId,
        CancellationToken cancellationToken = default);

    /// <summary>Gets one active question with its categories and answer choices.</summary>
    Task<Question?> GetByIdWithAnswersAsync(
        int questionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the active question assigned to the supplied local date, including
    /// its categories and answers.
    /// </summary>
    Task<Question?> GetDailyQuestionByDateAsync(
        DateOnly localDate,
        CancellationToken cancellationToken = default);

    /// <summary>Determines whether a question is assigned to the supplied local date.</summary>
    Task<bool> IsDailyQuestionAsync(
        int questionId,
        DateOnly date,
        CancellationToken cancellationToken = default);

    /// <summary>Determines whether an answer belongs to the specified question.</summary>
    Task<bool> AnswerBelongsToQuestionAsync(
        int answerId,
        int questionId,
        CancellationToken cancellationToken = default);
}
