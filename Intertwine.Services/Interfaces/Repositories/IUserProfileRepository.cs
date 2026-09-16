using Intertwine.Domain.Entities;

namespace Intertwine.Services.Interfaces.Repositories;

public interface IUserProfileRepository
{
    Task<UserProfile?> GetByIdAsync(int userProfileId);

    Task<UserProfile?> GetByIdentityUserIdAsync(string identityUserId);

    Task<UserProfile?> GetByIdentityUserIdIncludingInactiveAsync(
        string identityUserId);

    Task<IEnumerable<UserProfile>> GetAllAsync();

    Task<UserProfile> AddAsync(UserProfile userProfile);

    Task UpdateAsync(UserProfile userProfile);

    Task SetIsActiveAsync(
        UserProfile userProfile,
        bool isActive,
        string changedBy);
}
