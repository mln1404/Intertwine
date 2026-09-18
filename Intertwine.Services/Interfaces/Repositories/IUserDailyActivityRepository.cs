using Intertwine.Domain.Entities;

namespace Intertwine.Services.Interfaces.Repositories;

/// <summary>
/// Provides persistence access to per-user, per-local-date answer activity.
/// </summary>
public interface IUserDailyActivityRepository
{
    /// <summary>Gets activity for one profile and local date.</summary>
    Task<UserDailyActivity?> GetByUserAndDateAsync(
        int userProfileId,
        DateOnly date,
        CancellationToken cancellationToken = default);

    /// <summary>Adds a new activity record to the current unit of work.</summary>
    Task<UserDailyActivity> AddAsync(
        UserDailyActivity activity,
        CancellationToken cancellationToken = default);
}
