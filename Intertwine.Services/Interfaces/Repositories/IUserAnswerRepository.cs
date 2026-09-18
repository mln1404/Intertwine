using Intertwine.Domain.Entities;

namespace Intertwine.Services.Interfaces.Repositories;

/// <summary>
/// Provides persistence access to current user answers.
/// </summary>
public interface IUserAnswerRepository
{
    /// <summary>Gets all current answers for one profile with question context.</summary>
    Task<IReadOnlyList<UserAnswers>> GetByUserProfileIdAsync(
        int userProfileId,
        CancellationToken cancellationToken = default);

    /// <summary>Gets the current answer for one profile and question.</summary>
    Task<UserAnswers?> GetByUserAndQuestionAsync(
        int userProfileId,
        int questionId,
        CancellationToken cancellationToken = default);

    /// <summary>Adds a current answer to the current unit of work.</summary>
    Task<UserAnswers> AddAsync(
        UserAnswers userAnswer,
        CancellationToken cancellationToken = default);
}
