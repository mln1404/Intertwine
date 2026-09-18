using Intertwine.Services.DTOs.Questions;
using Intertwine.Services.DTOs.UserAnswers;

namespace Intertwine.Services.Interfaces.Services;

/// <summary>
/// Applies the business rules for a user's current question answers.
/// </summary>
public interface IUserAnswerService
{
    /// <summary>Gets the caller's current selected answer for each answered question.</summary>
    Task<IReadOnlyList<UserAnswerSelectionDto>> GetCurrentAnswersAsync(
        string identityUserId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates or changes an answer while enforcing daily limits and any required Spark charge.
    /// </summary>
    Task SubmitAnswerAsync(
        string identityUserId,
        int questionId,
        SubmitAnswerRequest request,
        DateOnly localDate,
        CancellationToken cancellationToken = default);
}
