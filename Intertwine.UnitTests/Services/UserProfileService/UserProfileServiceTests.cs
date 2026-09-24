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
        var service = CreateService(repository);

        var result = await service.GetCurrentUserProfileAsync(profile.IdentityUserId);

        Assert.NotNull(result);
        Assert.Equal(profile.UserProfileId, result.UserProfileId);
        Assert.Equal(profile.IdentityUserId, result.IdentityUserId);
        Assert.Equal(profile.AvatarName, result.AvatarName);
        Assert.Equal(profile.FirstName, result.FirstName);
        Assert.Equal(profile.MiddleName, result.MiddleName);
        Assert.Equal(profile.LastName, result.LastName);
        Assert.Equal(profile.UserWallet!.CreditBalance, result.CreditBalance);
        Assert.Equal(profile.PersonalityTypeId, result.PersonalityTypeId);
        Assert.Equal(profile.PersonalityType!.Code, result.PersonalityTypeCode);
    }

    [Fact]
    public async Task GetCurrentUserProfileAsync_WhenProfileDoesNotExist_ReturnsNull()
    {
        var repository = CreateRepositoryReturning(null);
        var service = CreateService(repository);

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
        var service = CreateService(repository);
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
        Assert.Null(result.PersonalityTypeId);
        Assert.Null(result.PersonalityTypeCode);
        repository.Verify(x => x.UpdateAsync(profile), Times.Once);
    }

    [Fact]
    public async Task UpdateCurrentUserAsync_WithActivePersonalityType_SavesAndReturnsCode()
    {
        var profile = CreateProfile();
        var repository = CreateRepositoryReturning(profile);
        repository.Setup(x => x.UpdateAsync(profile))
            .Returns(Task.CompletedTask);
        var personalityType = new PersonalityType
        {
            PersonalityTypeId = 8,
            Code = "ENFP",
            IsActive = true
        };
        var personalityTypes = new Mock<IPersonalityTypeRepository>();
        personalityTypes.Setup(x => x.GetActiveByIdAsync(8, It.IsAny<CancellationToken>()))
            .ReturnsAsync(personalityType);
        var service = CreateService(repository, personalityTypes);

        var result = await service.UpdateCurrentUserAsync(
            profile.IdentityUserId,
            new UpdateUserProfileRequest
            {
                AvatarName = profile.AvatarName,
                FirstName = profile.FirstName,
                MiddleName = profile.MiddleName,
                LastName = profile.LastName,
                PersonalityTypeId = 8
            });

        Assert.NotNull(result);
        Assert.Equal(8, result.PersonalityTypeId);
        Assert.Equal("ENFP", result.PersonalityTypeCode);
        Assert.Equal(8, profile.PersonalityTypeId);
        repository.Verify(x => x.UpdateAsync(profile), Times.Once);
    }

    [Fact]
    public async Task UpdateCurrentUserAsync_WithInvalidPersonalityType_RejectsWithoutUpdate()
    {
        var profile = CreateProfile();
        var repository = CreateRepositoryReturning(profile);
        var personalityTypes = new Mock<IPersonalityTypeRepository>();
        personalityTypes.Setup(x => x.GetActiveByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PersonalityType?)null);
        var service = CreateService(repository, personalityTypes);

        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.UpdateCurrentUserAsync(
                profile.IdentityUserId,
                new UpdateUserProfileRequest { PersonalityTypeId = 999 }));

        Assert.Contains("does not exist or is inactive", exception.Message);
        repository.Verify(x => x.UpdateAsync(It.IsAny<UserProfile>()), Times.Never);
    }

    [Fact]
    public async Task UpdateCurrentUserAsync_WhenProfileDoesNotExist_ReturnsNullWithoutUpdate()
    {
        var repository = CreateRepositoryReturning(null);
        var service = CreateService(repository);

        var result = await service.UpdateCurrentUserAsync(
            "missing-user",
            new UpdateUserProfileRequest());

        Assert.Null(result);
        repository.Verify(
            x => x.UpdateAsync(It.IsAny<UserProfile>()),
            Times.Never);
    }

    [Fact]
    public async Task DeactivateCurrentUserAsync_WhenProfileExists_SetsProfileInactive()
    {
        var profile = CreateProfile();
        var repository = CreateRepositoryReturning(profile);
        repository.Setup(x => x.SetIsActiveAsync(profile, false, profile.IdentityUserId))
            .Returns(Task.CompletedTask);
        var service = CreateService(repository);

        var result = await service.DeactivateCurrentUserAsync(profile.IdentityUserId);

        Assert.True(result);
        repository.Verify(
            x => x.SetIsActiveAsync(profile, false, profile.IdentityUserId),
            Times.Once);
    }

    [Fact]
    public async Task DeactivateCurrentUserAsync_WhenProfileDoesNotExist_ReturnsFalse()
    {
        var repository = CreateRepositoryReturning(null);
        var service = CreateService(repository);

        var result = await service.DeactivateCurrentUserAsync("missing-user");

        Assert.False(result);
        repository.Verify(
            x => x.SetIsActiveAsync(
                It.IsAny<UserProfile>(),
                It.IsAny<bool>(),
                It.IsAny<string>()),
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

    private static Service CreateService(
        Mock<IUserProfileRepository> profiles,
        Mock<IPersonalityTypeRepository>? personalityTypes = null) =>
        new(
            profiles.Object,
            (personalityTypes ?? new Mock<IPersonalityTypeRepository>()).Object);

    private static UserProfile CreateProfile() => new()
    {
        UserProfileId = 12,
        IdentityUserId = "identity-12",
        AvatarName = "avatar",
        FirstName = "First",
        MiddleName = "Middle",
        LastName = "Last",
        IsActive = true,
        PersonalityTypeId = 1,
        PersonalityType = new PersonalityType
        {
            PersonalityTypeId = 1,
            Code = "INTJ",
            IsActive = true
        },
        UserWallet = new UserWallet
        {
            CreditBalance = 125
        }
    };
}
