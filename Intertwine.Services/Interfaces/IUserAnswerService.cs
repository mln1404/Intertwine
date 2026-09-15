using Intertwine.Services.DTOs.Questions;
using Intertwine.Services.DTOs.UserAnswers;

namespace Intertwine.Services.Interfaces.Services;

public interface IUserAnswerService
{
    Task<IReadOnlyList<UserAnswerSelectionDto>> GetCurrentAnswersAsync(
        string identityUserId,
        CancellationToken cancellationToken = default);

    Task SubmitAnswerAsync(
        string identityUserId,
        int questionId,
        SubmitAnswerRequest request,
        DateOnly localDate,
        CancellationToken cancellationToken = default);
}
