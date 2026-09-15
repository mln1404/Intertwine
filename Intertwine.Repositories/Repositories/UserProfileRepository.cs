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

    public async Task<UserProfile?> GetByIdAsync(int userProfileId)
    {
        return await _context.UserProfiles
            .AsNoTracking()
            .Include(x => x.UserAnswers)
            .Include(x => x.UserWallet)
            .FirstOrDefaultAsync(x => x.UserProfileId == userProfileId);
    }

    public async Task<UserProfile?> GetByIdentityUserIdAsync(
        string identityUserId)
    {
        return await _context.UserProfiles
            .Include(x => x.UserAnswers)
            .Include(x => x.UserWallet)
            .FirstOrDefaultAsync(
                x => x.IdentityUserId == identityUserId);
    }

    public async Task<IEnumerable<UserProfile>> GetAllAsync()
    {
        return await _context.UserProfiles
            .AsNoTracking()
            .Include(x => x.UserAnswers)
            .Include(x => x.UserWallet)
            .ToListAsync();
    }

    public async Task<UserProfile> AddAsync(UserProfile userProfile)
    {
        await _context.UserProfiles.AddAsync(userProfile);
        await _context.SaveChangesAsync();

        return userProfile;
    }

    public async Task UpdateAsync(UserProfile userProfile)
    {
        _context.UserProfiles.Update(userProfile);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(UserProfile userProfile)
    {
        _context.UserProfiles.Remove(userProfile);
        await _context.SaveChangesAsync();
    }
}
