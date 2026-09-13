using Intertwine.Domain.Entities;

namespace Intertwine.Services.Interfaces.Repositories;

public interface IUserAnswerRepository
{
    Task<UserAnswers> AddAsync(
        UserAnswers userAnswer,
        CancellationToken cancellationToken = default);
}
