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
}
