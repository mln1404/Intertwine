using Intertwine.Services.DTOs.UserProfiles;

namespace Intertwine.Services.Interfaces;

public interface IUserProfileService
{
    Task<UserProfileDto?> GetCurrentUserProfileAsync(
        string identityUserId);

    Task<UserProfileDto?> UpdateCurrentUserAsync(
        string identityUserId,
        UpdateUserProfileRequest request);

    Task<bool> DeleteCurrentUserAsync(
        string identityUserId);
}