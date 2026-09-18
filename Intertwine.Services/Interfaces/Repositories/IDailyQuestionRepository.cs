using Intertwine.Domain.Entities;
using Intertwine.Services.DTOs.Questions;

namespace Intertwine.Services.Interfaces.Repositories;

/// <summary>
/// Provides persistence access to Daily Question assignments.
/// </summary>
public interface IDailyQuestionRepository
{
    /// <summary>Determines whether a local date already has an assignment.</summary>
    Task<bool> ExistsAsync(DateOnly date, CancellationToken cancellationToken = default);

    /// <summary>Gets active candidates and their prior assignment usage.</summary>
    Task<DailyQuestionSelection> GetSelectionAsync(CancellationToken cancellationToken = default);

    /// <summary>Persists an assignment; returns false if another caller already filled the date.</summary>
    Task<bool> TryInsertAsync(DailyQuestion assignment, CancellationToken cancellationToken = default);
}
