using Intertwine.Domain.Entities;
using Intertwine.Repositories.Data;
using Intertwine.Services.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Intertwine.Repositories.Repositories;

/// <summary>
/// Provides database operations for user profiles.
/// </summary>
public class UserProfileRepository : IUserProfileRepository
{
    private readonly IntertwineDbContext _context;

    public UserProfileRepository(IntertwineDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<UserProfile?> GetByIdAsync(int userProfileId)
    {
        return await ActiveProfiles()
            .AsNoTracking()
            .Include(x => x.UserAnswers)
            .Include(x => x.UserWallet)
            .FirstOrDefaultAsync(x => x.UserProfileId == userProfileId);
    }

    /// <inheritdoc />
    public async Task<UserProfile?> GetByIdentityUserIdAsync(
        string identityUserId)
    {
        return await ActiveProfiles()
            .Include(x => x.UserWallet)
            .FirstOrDefaultAsync(
                x => x.IdentityUserId == identityUserId);
    }

    /// <inheritdoc />
    public async Task<UserProfile?> GetByIdentityUserIdIncludingInactiveAsync(
        string identityUserId)
    {
        return await _context.UserProfiles
            .Include(x => x.UserWallet)
            .FirstOrDefaultAsync(
                x => x.IdentityUserId == identityUserId);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<UserProfile>> GetAllAsync()
    {
        return await ActiveProfiles()
            .AsNoTracking()
            .Include(x => x.UserAnswers)
            .Include(x => x.UserWallet)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<UserProfile> AddAsync(UserProfile userProfile)
    {
        await _context.UserProfiles.AddAsync(userProfile);
        await _context.SaveChangesAsync();

        return userProfile;
    }

    /// <inheritdoc />
    public async Task UpdateAsync(UserProfile userProfile)
    {
        _context.UserProfiles.Update(userProfile);
        await _context.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task SetIsActiveAsync(
        UserProfile userProfile,
        bool isActive,
        string changedBy)
    {
        userProfile.IsActive = isActive;
        userProfile.DateUpdated = DateTime.UtcNow;
        userProfile.UpdatedBy = changedBy;
        await _context.SaveChangesAsync();
    }

    private IQueryable<UserProfile> ActiveProfiles() =>
        _context.UserProfiles.Where(x => x.IsActive);
}
