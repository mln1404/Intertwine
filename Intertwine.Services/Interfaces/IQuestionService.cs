using Intertwine.Services.DTOs.Questions;
using Intertwine.Services.DTOs.UserAnswers;

namespace Intertwine.Services.Interfaces;

public interface IQuestionService
{
    Task<IEnumerable<QuestionListDto>> GetQuestionsAsync(
        int? categoryId = null,
        CancellationToken cancellationToken = default);

    Task<QuestionDetailDto?> GetQuestionByIdAsync(
        int questionId,
        CancellationToken cancellationToken = default);

    Task<bool> SubmitAnswerAsync(
        string identityUserId,
        int questionId,
        SubmitAnswerRequest request,
        CancellationToken cancellationToken = default);
}
