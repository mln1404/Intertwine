using Intertwine.Domain.Entities;

namespace Intertwine.Services.Interfaces.Repositories;

public interface IUserDailyActivityRepository
{
    Task<UserDailyActivity?> GetByUserAndDateAsync(
        int userProfileId,
        DateOnly date,
        CancellationToken cancellationToken = default);

    Task<UserDailyActivity> AddAsync(
        UserDailyActivity activity,
        CancellationToken cancellationToken = default);
}
