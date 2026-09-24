using Intertwine.Domain.Entities;

namespace Intertwine.Services.Interfaces.Repositories;

/// <summary>
/// Provides persistence access to application profiles.
/// </summary>
public interface IUserProfileRepository
{
    /// <summary>Gets a profile by its database identifier.</summary>
    Task<UserProfile?> GetByIdAsync(int userProfileId);

    /// <summary>Gets an active profile by the linked ASP.NET Identity user identifier.</summary>
    Task<UserProfile?> GetByIdentityUserIdAsync(string identityUserId);

    /// <summary>Gets a profile regardless of its active state.</summary>
    Task<UserProfile?> GetByIdentityUserIdIncludingInactiveAsync(
        string identityUserId);

    /// <summary>
    /// Gets the active profile data required for its public presentation.
    /// </summary>
    Task<UserProfile?> GetPublicProfileByIdentityUserIdAsync(
        string identityUserId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an active profile by its database identifier with the related data
    /// required to build its public presentation.
    /// </summary>
    Task<UserProfile?> GetPublicProfileByIdAsync(
        int userProfileId,
        CancellationToken cancellationToken = default);

    /// <summary>Gets all profiles.</summary>
    Task<IEnumerable<UserProfile>> GetAllAsync();

    /// <summary>Adds a profile to the current unit of work.</summary>
    Task<UserProfile> AddAsync(UserProfile userProfile);

    /// <summary>Marks an existing profile as modified in the current unit of work.</summary>
    Task UpdateAsync(UserProfile userProfile);

    /// <summary>Changes whether the profile can use the application.</summary>
    Task SetIsActiveAsync(
        UserProfile userProfile,
        bool isActive,
        string changedBy);
}
