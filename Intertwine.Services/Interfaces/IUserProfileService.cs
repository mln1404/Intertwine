using Intertwine.Services.DTOs.UserProfiles;

namespace Intertwine.Services.Interfaces;

public interface IUserProfileService
{
    Task<UserProfileDto?> CreateCurrentUserAsync(
        string identityUserId,
        CreateUserProfileRequest request);

    Task<UserProfileDto?> GetCurrentUserProfileAsync(
        string identityUserId);

    Task<UserProfileDto?> UpdateCurrentUserAsync(
        string identityUserId,
        UpdateUserProfileRequest request);

    Task<bool> DeactivateCurrentUserAsync(
        string identityUserId);
}
