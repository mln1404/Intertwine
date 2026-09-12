using Intertwine.Services.DTOs.UserProfiles;

namespace Intertwine.Services.Interfaces
{
    public interface IUserProfileService
    {

        public Task<UserProfileDto?> GetByIdAsync(int userProfileId);
        public Task<UserProfileDto?> GetCurrentUserProfileAsync(string identityUserId);

        public Task<IEnumerable<UserProfileDto>> GetAllAsync();

        public Task<UserProfileDto?> CreateAsync(CreateUserProfileRequest request);

        public Task<UserProfileDto?> UpdateAsync(int userProfileId, UpdateUserProfileRequest request);

        public Task<bool> DeleteAsync(int userProfileId);
    }
}
