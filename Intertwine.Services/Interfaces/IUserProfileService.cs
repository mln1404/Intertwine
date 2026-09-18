using Intertwine.Services.DTOs.UserProfiles;

namespace Intertwine.Services.Interfaces;

/// <summary>
/// Manages the profile associated with an identity account.
/// </summary>
public interface IUserProfileService
{
    /// <summary>Creates a profile for the current identity when one does not exist.</summary>
    Task<UserProfileDto?> CreateCurrentUserAsync(
        string identityUserId,
        CreateUserProfileRequest request);

    /// <summary>Gets the active profile for the current identity.</summary>
    Task<UserProfileDto?> GetCurrentUserProfileAsync(
        string identityUserId);

    /// <summary>Updates the editable fields of the current profile.</summary>
    Task<UserProfileDto?> UpdateCurrentUserAsync(
        string identityUserId,
        UpdateUserProfileRequest request);

    /// <summary>Marks the current profile inactive without deleting its data.</summary>
    Task<bool> DeactivateCurrentUserAsync(
        string identityUserId);
}
