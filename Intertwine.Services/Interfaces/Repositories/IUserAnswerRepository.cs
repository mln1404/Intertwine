using Intertwine.Domain.Entities;

namespace Intertwine.Services.Interfaces.Repositories;

public interface IUserAnswerRepository
{
    Task<IReadOnlyList<UserAnswers>> GetByUserProfileIdAsync(
        int userProfileId,
        CancellationToken cancellationToken = default);

    Task<UserAnswers?> GetByUserAndQuestionAsync(
        int userProfileId,
        int questionId,
        CancellationToken cancellationToken = default);

    Task<UserAnswers> AddAsync(
        UserAnswers userAnswer,
        CancellationToken cancellationToken = default);
}
