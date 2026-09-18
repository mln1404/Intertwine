using Intertwine.Services.DTOs.UserDailyActivities;

namespace Intertwine.Services.Interfaces;

/// <summary>
/// Retrieves the answer allowance and Daily Question state for a user's local date.
/// </summary>
public interface IUserDailyActivityService
{
    /// <summary>Gets or initializes activity for the supplied local calendar date.</summary>
    Task<UserDailyActivityDto> GetCurrentUserActivityAsync(
        string identityUserId,
        DateOnly localDate,
        CancellationToken cancellationToken = default);
}
