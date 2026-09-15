using Intertwine.Services.DTOs.UserDailyActivities;

namespace Intertwine.Services.Interfaces;

public interface IUserDailyActivityService
{
    Task<UserDailyActivityDto> GetCurrentUserActivityAsync(
        string identityUserId,
        DateOnly localDate,
        CancellationToken cancellationToken = default);
}
