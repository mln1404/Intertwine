using Intertwine.Services.DTOs.Questions;

namespace Intertwine.Services.Interfaces;

public interface IQuestionService
{
    Task<IEnumerable<QuestionListDto>> GetQuestionsAsync(
        int? categoryId = null,
        CancellationToken cancellationToken = default);

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
