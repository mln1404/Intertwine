using Intertwine.Domain.Entities;
using Intertwine.Services.DTOs.UserProfiles;
using Intertwine.Services.Interfaces.Repositories;
using Moq;
using Service = Intertwine.Services.Services.UserProfileService;

namespace Intertwine.UnitTests.Services.UserProfileService;

public class CreateUserProfileTests
{
    [Fact]
    public async Task CreateCurrentUserAsync_WhenProfileDoesNotExist_CreatesProfileAndWallet()
    {
        var repository = new Mock<IUserProfileRepository>();
        repository.Setup(x => x.GetByIdentityUserIdIncludingInactiveAsync("identity-new"))
            .ReturnsAsync((UserProfile?)null);
        repository.Setup(x => x.AddAsync(It.IsAny<UserProfile>()))
            .ReturnsAsync((UserProfile profile) => profile);
        var service = new Service(repository.Object);
        var request = new CreateUserProfileRequest
        {
            AvatarName = "new-avatar",
            FirstName = "New",
            MiddleName = "Middle",
            LastName = "Person"
        };

        var result = await service.CreateCurrentUserAsync("identity-new", request);

        Assert.NotNull(result);
        Assert.Equal("identity-new", result.IdentityUserId);
        Assert.Equal(request.AvatarName, result.AvatarName);
        Assert.Equal(request.FirstName, result.FirstName);
        Assert.Equal(request.MiddleName, result.MiddleName);
        Assert.Equal(request.LastName, result.LastName);
        Assert.Equal(0, result.CreditBalance);
        repository.Verify(x => x.AddAsync(
            It.Is<UserProfile>(profile =>
                profile.IdentityUserId == "identity-new" &&
                profile.UserWallet != null &&
                profile.UserWallet.CreditBalance == 0 &&
                profile.CreatedBy == "identity-new" &&
                profile.UserWallet.CreatedBy == "identity-new")),
            Times.Once);
    }

    [Fact]
    public async Task CreateCurrentUserAsync_WhenInactiveProfileExists_ReturnsNullWithoutCreating()
    {
        var repository = new Mock<IUserProfileRepository>();
        repository.Setup(x => x.GetByIdentityUserIdIncludingInactiveAsync("identity-existing"))
            .ReturnsAsync(new UserProfile
            {
                IdentityUserId = "identity-existing",
                IsActive = false
            });
        var service = new Service(repository.Object);

        var result = await service.CreateCurrentUserAsync(
            "identity-existing",
            new CreateUserProfileRequest());

        Assert.Null(result);
        repository.Verify(
            x => x.AddAsync(It.IsAny<UserProfile>()),
            Times.Never);
    }
}
