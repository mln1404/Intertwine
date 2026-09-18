using Intertwine.Domain.Entities;
using Intertwine.Services.DTOs.UserProfiles;
using Intertwine.Services.Interfaces;
using Intertwine.Services.Interfaces.Repositories;

namespace Intertwine.Services.Services;

/// <summary>
/// Manages creation, retrieval, editing, and deactivation of application profiles.
/// </summary>
public class UserProfileService : IUserProfileService
{
    private readonly IUserProfileRepository _userProfileRepository;

    public UserProfileService(
        IUserProfileRepository userProfileRepository)
    {
        _userProfileRepository = userProfileRepository;
    }

    /// <inheritdoc />
    public async Task<UserProfileDto?> CreateCurrentUserAsync(
        string identityUserId,
        CreateUserProfileRequest request)
    {
        var existingProfile = await _userProfileRepository
            .GetByIdentityUserIdIncludingInactiveAsync(identityUserId);

        if (existingProfile is not null)
            return null;

        var userProfile = new UserProfile
        {
            IdentityUserId = identityUserId,
            AvatarName = request.AvatarName,
            FirstName = request.FirstName,
            LastName = request.LastName,
            MiddleName = request.MiddleName,
            DateCreated = DateTime.UtcNow,
            CreatedBy = identityUserId,
            UserWallet = new UserWallet
            {
                CreditBalance = 0,
                DateCreated = DateTime.UtcNow,
                CreatedBy = identityUserId
            }
        };

        await _userProfileRepository.AddAsync(userProfile);

        return MapToDto(userProfile);
    }

    /// <inheritdoc />
    public async Task<UserProfileDto?> GetCurrentUserProfileAsync(
        string identityUserId)
    {
        var userProfile = await _userProfileRepository
            .GetByIdentityUserIdAsync(identityUserId);

        if (userProfile is null)
            return null;

        return MapToDto(userProfile);
    }

    /// <inheritdoc />
    public async Task<UserProfileDto?> UpdateCurrentUserAsync(
        string identityUserId,
        UpdateUserProfileRequest request)
    {
        var userProfile = await _userProfileRepository
            .GetByIdentityUserIdAsync(identityUserId);

        if (userProfile is null)
            return null;

        userProfile.AvatarName = request.AvatarName;
        userProfile.FirstName = request.FirstName;
        userProfile.LastName = request.LastName;
        userProfile.MiddleName = request.MiddleName;

        await _userProfileRepository.UpdateAsync(userProfile);

        return MapToDto(userProfile);
    }

    /// <inheritdoc />
    public async Task<bool> DeactivateCurrentUserAsync(
        string identityUserId)
    {
        var userProfile = await _userProfileRepository
            .GetByIdentityUserIdAsync(identityUserId);

        if (userProfile is null)
            return false;

        await _userProfileRepository.SetIsActiveAsync(
            userProfile,
            false,
            identityUserId);

        return true;
    }

    private static UserProfileDto MapToDto(
        UserProfile userProfile)
    {
        return new UserProfileDto
        {
            UserProfileId = userProfile.UserProfileId,
            AvatarName = userProfile.AvatarName,
            FirstName = userProfile.FirstName,
            LastName = userProfile.LastName,
            MiddleName = userProfile.MiddleName,
            IdentityUserId = userProfile.IdentityUserId,
            CreditBalance = userProfile.UserWallet?.CreditBalance ?? 0
        };
    }
}
