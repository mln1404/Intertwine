using Intertwine.Domain.Entities;
using Intertwine.Services.DTOs.Questions;

namespace Intertwine.Services.Interfaces.Repositories;

public interface IDailyQuestionRepository
{
    Task<bool> ExistsAsync(DateOnly date, CancellationToken cancellationToken = default);

    Task<DailyQuestionSelection> GetSelectionAsync(CancellationToken cancellationToken = default);

    /// <summary>Persists an assignment; returns false if another caller already filled the date.</summary>
    Task<bool> TryInsertAsync(DailyQuestion assignment, CancellationToken cancellationToken = default);
}
