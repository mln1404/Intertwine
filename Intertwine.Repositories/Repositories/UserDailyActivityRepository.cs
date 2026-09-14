using Intertwine.Domain.Entities;
using Intertwine.Repositories.Data;
using Intertwine.Services.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Intertwine.Repositories.Repositories;

/// <summary>
/// Provides tracked access to per-user, per-date activity records.
/// </summary>
public class UserDailyActivityRepository
    : IUserDailyActivityRepository
{
    private readonly IntertwineDbContext _context;

    public UserDailyActivityRepository(
        IntertwineDbContext context)
    {
        _context = context;
    }

    public async Task<UserDailyActivity?> GetByUserAndDateAsync(
        int userProfileId,
        DateOnly date,
        CancellationToken cancellationToken = default)
    {
        return await _context.UserDailyActivities
            .FirstOrDefaultAsync(
                x =>
                    x.UserProfileId == userProfileId &&
                    x.Date == date,
                cancellationToken);
    }

    public async Task<UserDailyActivity> AddAsync(
        UserDailyActivity activity,
        CancellationToken cancellationToken = default)
    {
        await _context.UserDailyActivities.AddAsync(
            activity,
            cancellationToken);

        return activity;
    }
}
