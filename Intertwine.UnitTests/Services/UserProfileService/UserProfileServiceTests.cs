using Intertwine.Domain.Entities;
using Intertwine.Services.DTOs.UserProfiles;
using Intertwine.Services.Interfaces.Repositories;
using Moq;
using Service = Intertwine.Services.Services.UserProfileService;

namespace Intertwine.UnitTests.Services.UserProfileService;

public class UserProfileServiceTests
{
    [Fact]
    public async Task GetCurrentUserProfileAsync_WhenProfileExists_MapsProfile()
    {
        var profile = CreateProfile();
        var repository = CreateRepositoryReturning(profile);
        var service = new Service(repository.Object);

        var result = await service.GetCurrentUserProfileAsync(profile.IdentityUserId);

        Assert.NotNull(result);
        Assert.Equal(profile.UserProfileId, result.UserProfileId);
        Assert.Equal(profile.IdentityUserId, result.IdentityUserId);
        Assert.Equal(profile.AvatarName, result.AvatarName);
        Assert.Equal(profile.FirstName, result.FirstName);
        Assert.Equal(profile.MiddleName, result.MiddleName);
        Assert.Equal(profile.LastName, result.LastName);
    }

    [Fact]
    public async Task GetCurrentUserProfileAsync_WhenProfileDoesNotExist_ReturnsNull()
    {
        var repository = CreateRepositoryReturning(null);
        var service = new Service(repository.Object);

        var result = await service.GetCurrentUserProfileAsync("missing-user");

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateCurrentUserAsync_WhenProfileExists_UpdatesAndReturnsProfile()
    {
        var profile = CreateProfile();
        var repository = CreateRepositoryReturning(profile);
        repository.Setup(x => x.UpdateAsync(profile))
            .Returns(Task.CompletedTask);
        var service = new Service(repository.Object);
        var request = new UpdateUserProfileRequest
        {
            AvatarName = "new-avatar",
            FirstName = "Updated",
            MiddleName = "New Middle",
            LastName = "Name"
        };

        var result = await service.UpdateCurrentUserAsync(
            profile.IdentityUserId,
            request);

        Assert.NotNull(result);
        Assert.Equal(request.AvatarName, result.AvatarName);
        Assert.Equal(request.FirstName, result.FirstName);
        Assert.Equal(request.MiddleName, result.MiddleName);
        Assert.Equal(request.LastName, result.LastName);
        repository.Verify(x => x.UpdateAsync(profile), Times.Once);
    }

    [Fact]
    public async Task UpdateCurrentUserAsync_WhenProfileDoesNotExist_ReturnsNullWithoutUpdate()
    {
        var repository = CreateRepositoryReturning(null);
        var service = new Service(repository.Object);

        var result = await service.UpdateCurrentUserAsync(
            "missing-user",
            new UpdateUserProfileRequest());

        Assert.Null(result);
        repository.Verify(
            x => x.UpdateAsync(It.IsAny<UserProfile>()),
            Times.Never);
    }

    [Fact]
    public async Task DeleteCurrentUserAsync_WhenProfileExists_DeletesAndReturnsTrue()
    {
        var profile = CreateProfile();
        var repository = CreateRepositoryReturning(profile);
        repository.Setup(x => x.DeleteAsync(profile))
            .Returns(Task.CompletedTask);
        var service = new Service(repository.Object);

        var result = await service.DeleteCurrentUserAsync(profile.IdentityUserId);

        Assert.True(result);
        repository.Verify(x => x.DeleteAsync(profile), Times.Once);
    }

    [Fact]
    public async Task DeleteCurrentUserAsync_WhenProfileDoesNotExist_ReturnsFalseWithoutDelete()
    {
        var repository = CreateRepositoryReturning(null);
        var service = new Service(repository.Object);

        var result = await service.DeleteCurrentUserAsync("missing-user");

        Assert.False(result);
        repository.Verify(
            x => x.DeleteAsync(It.IsAny<UserProfile>()),
            Times.Never);
    }

    private static Mock<IUserProfileRepository> CreateRepositoryReturning(
        UserProfile? profile)
    {
        var repository = new Mock<IUserProfileRepository>();
        repository.Setup(x => x.GetByIdentityUserIdAsync(It.IsAny<string>()))
            .ReturnsAsync(profile);
        return repository;
    }

    private static UserProfile CreateProfile() => new()
    {
        UserProfileId = 12,
        IdentityUserId = "identity-12",
        AvatarName = "avatar",
        FirstName = "First",
        MiddleName = "Middle",
        LastName = "Last"
    };
}
