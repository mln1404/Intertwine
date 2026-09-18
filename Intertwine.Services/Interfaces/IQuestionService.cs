using Intertwine.Services.DTOs.Questions;

namespace Intertwine.Services.Interfaces;

/// <summary>
/// Retrieves active questions and their answer choices.
/// </summary>
public interface IQuestionService
{
    /// <summary>Gets active questions, optionally narrowed to one category.</summary>
    Task<IEnumerable<QuestionListDto>> GetQuestionsAsync(
        int? categoryId = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets one active question with its categories and answer choices.</summary>
    Task<QuestionDetailDto?> GetQuestionByIdAsync(
        int questionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the Daily Question assigned to a local calendar date.
    /// </summary>
    Task<QuestionDetailDto?> GetDailyQuestionAsync(
        DateOnly localDate,
        CancellationToken cancellationToken = default);
}
