using Intertwine.Domain.Entities;

namespace Intertwine.Services.Interfaces.Repositories;

public interface IQuestionRepository
{
    Task<IEnumerable<Question>> GetAllActiveAsync(
        CancellationToken cancellationToken = default);

    Task<IEnumerable<Question>> GetByCategoryAsync(
        int categoryId,
        CancellationToken cancellationToken = default);

    Task<Question?> GetByIdWithAnswersAsync(
        int questionId,
        CancellationToken cancellationToken = default);

    Task<bool> IsDailyQuestionAsync(
        int questionId,
        DateOnly date,
        CancellationToken cancellationToken = default);

    Task<bool> AnswerBelongsToQuestionAsync(
        int answerId,
        int questionId,
        CancellationToken cancellationToken = default);
}
