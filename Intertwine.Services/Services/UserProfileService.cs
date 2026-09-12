using Intertwine.Domain.Entities;
using Intertwine.Services.DTOs.UserProfiles;
using Intertwine.Services.Interfaces;
using Intertwine.Services.Interfaces.Repositories;

namespace Intertwine.Services.Services;

public class UserProfileService : IUserProfileService
{
    private readonly IUserProfileRepository _userProfileRepository;

    public UserProfileService(
        IUserProfileRepository userProfileRepository)
    {
        _userProfileRepository = userProfileRepository;
    }

    public async Task<UserProfileDto?> GetByIdAsync(
        int userProfileId)
    {
        var userProfile =
            await _userProfileRepository.GetByIdAsync(userProfileId);

        if (userProfile is null)
        {
            return null;
        }

        return MapToDto(userProfile);
    }

    public async Task<UserProfileDto?> GetCurrentUserProfileAsync(
        string identityUserId)
    {
        var userProfile =
            await _userProfileRepository
                .GetByIdentityUserIdAsync(identityUserId);

        if (userProfile is null)
        {
            return null;
        }

        return MapToDto(userProfile);
    }

    public async Task<IEnumerable<UserProfileDto>> GetAllAsync()
    {
        var userProfiles =
            await _userProfileRepository.GetAllAsync();

        return userProfiles.Select(MapToDto);
    }

    public async Task<UserProfileDto?> CreateAsync(
        CreateUserProfileRequest request)
    {
        var existingProfile =
            await _userProfileRepository
                .GetByIdentityUserIdAsync(request.IdentityUserId);

        if (existingProfile is not null)
        {
            return null;
        }

        var userProfile = new UserProfile
        {
            AvatarName = request.AvatarName,
            FirstName = request.FirstName,
            LastName = request.LastName,
            MiddleName = request.MiddleName,
            IdentityUserId = request.IdentityUserId
        };

        var createdProfile =
            await _userProfileRepository.AddAsync(userProfile);

        return MapToDto(createdProfile);
    }

    public async Task<UserProfileDto?> UpdateAsync(
        int userProfileId,
        UpdateUserProfileRequest request)
    {
        var userProfile =
            await _userProfileRepository.GetByIdAsync(userProfileId);

        if (userProfile is null)
        {
            return null;
        }

        userProfile.AvatarName = request.AvatarName;
        userProfile.FirstName = request.FirstName;
        userProfile.LastName = request.LastName;
        userProfile.MiddleName = request.MiddleName;

        await _userProfileRepository.UpdateAsync(userProfile);

        return MapToDto(userProfile);
    }

    public async Task<bool> DeleteAsync(int userProfileId)
    {
        var userProfile =
            await _userProfileRepository.GetByIdAsync(userProfileId);

        if (userProfile is null)
        {
            return false;
        }

        await _userProfileRepository.DeleteAsync(userProfile);

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
            IdentityUserId = userProfile.IdentityUserId
        };
    }
}