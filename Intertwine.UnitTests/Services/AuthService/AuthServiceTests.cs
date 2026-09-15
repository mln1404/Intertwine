using Intertwine.Domain.Entities;
using Intertwine.Identity;
using Intertwine.Services.Constants;
using Intertwine.Services.DTOs.Authentication;
using Intertwine.Services.Interfaces;
using Intertwine.Services.Interfaces.Repositories;
using Microsoft.AspNetCore.Identity;
using Moq;
using Service = Intertwine.Services.Services.AuthService;

namespace Intertwine.UnitTests.Services.AuthService;

public class AuthServiceTests
{
    [Fact]
    public async Task RegisterAsync_WhenEmailExists_ReturnsDuplicateEmailError()
    {
        var userManager = CreateUserManager();
        userManager.Setup(x => x.FindByEmailAsync("person@example.com"))
            .ReturnsAsync(new ApplicationUser());
        var service = CreateService(userManager);

        var result = await service.RegisterAsync(CreateRegisterRequest());

        Assert.False(result.Succeeded);
        Assert.Equal(AuthMessages.DuplicateEmail, result.Error);
        userManager.Verify(
            x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_WhenIdentityCreationFails_ReturnsCombinedErrors()
    {
        var userManager = CreateUserManager();
        userManager.Setup(x => x.FindByEmailAsync("person@example.com"))
            .ReturnsAsync((ApplicationUser?)null);
        userManager.Setup(x => x.CreateAsync(
                It.Is<ApplicationUser>(u =>
                    u.Email == "person@example.com" &&
                    u.UserName == "person@example.com"),
                "Password1!"))
            .ReturnsAsync(IdentityResult.Failed(
                new IdentityError { Description = "First error" },
                new IdentityError { Description = "Second error" }));
        var service = CreateService(userManager);

        var result = await service.RegisterAsync(CreateRegisterRequest());

        Assert.False(result.Succeeded);
        Assert.Equal(
            $"First error{AuthMessages.ErrorDelimiter}Second error",
            result.Error);
    }

    [Fact]
    public async Task RegisterAsync_WhenIdentityCreationSucceeds_ReturnsUserId()
    {
        var userManager = CreateUserManager();
        var userProfileRepository = new Mock<IUserProfileRepository>();
        userManager.Setup(x => x.FindByEmailAsync("person@example.com"))
            .ReturnsAsync((ApplicationUser?)null);
        userManager.Setup(x => x.CreateAsync(
                It.IsAny<ApplicationUser>(),
                "Password1!"))
            .Callback<ApplicationUser, string>((user, _) => user.Id = "identity-1")
            .ReturnsAsync(IdentityResult.Success);
        userProfileRepository.Setup(x => x.AddAsync(It.IsAny<UserProfile>()))
            .ReturnsAsync((UserProfile profile) => profile);
        var service = CreateService(
            userManager,
            userProfileRepository: userProfileRepository);

        var result = await service.RegisterAsync(CreateRegisterRequest());

        Assert.True(result.Succeeded);
        Assert.Equal("identity-1", result.UserId);
        Assert.Null(result.Error);
        userProfileRepository.Verify(x => x.AddAsync(
            It.Is<UserProfile>(profile =>
                profile.IdentityUserId == "identity-1" &&
                profile.FirstName == "Test" &&
                profile.LastName == "Person" &&
                profile.UserWallet != null &&
                profile.UserWallet.CreditBalance == 0)),
            Times.Once);
    }

    [Theory]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public async Task LoginAsync_WhenCredentialsAreInvalid_ReturnsGenericError(
        bool userExists,
        bool passwordIsValid)
    {
        var userManager = CreateUserManager();
        var user = userExists
            ? new ApplicationUser
            {
                Id = "identity-1",
                Email = "person@example.com"
            }
            : null;
        userManager.Setup(x => x.FindByEmailAsync("person@example.com"))
            .ReturnsAsync(user);
        if (userExists)
        {
            userManager.Setup(x => x.CheckPasswordAsync(user!, "Password1!"))
                .ReturnsAsync(passwordIsValid);
        }

        var tokenService = new Mock<ITokenService>(MockBehavior.Strict);
        var service = CreateService(userManager, tokenService);

        var result = await service.LoginAsync(new LoginRequest
        {
            Email = "person@example.com",
            Password = "Password1!"
        });

        Assert.False(result.Succeeded);
        Assert.Equal(AuthMessages.InvalidCredentials, result.Error);
        tokenService.Verify(
            x => x.GenerateToken(It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task LoginAsync_WhenCredentialsAreValid_ReturnsGeneratedToken()
    {
        var user = new ApplicationUser
        {
            Id = "identity-1",
            Email = "person@example.com"
        };
        var userManager = CreateUserManager();
        userManager.Setup(x => x.FindByEmailAsync(user.Email))
            .ReturnsAsync(user);
        userManager.Setup(x => x.CheckPasswordAsync(user, "Password1!"))
            .ReturnsAsync(true);
        var tokenService = new Mock<ITokenService>(MockBehavior.Strict);
        tokenService.Setup(x => x.GenerateToken(user.Id, user.Email))
            .Returns("signed-token");
        var service = CreateService(userManager, tokenService);

        var result = await service.LoginAsync(new LoginRequest
        {
            Email = user.Email,
            Password = "Password1!"
        });

        Assert.True(result.Succeeded);
        Assert.Equal(user.Id, result.UserId);
        Assert.Equal("signed-token", result.Token);
        Assert.Null(result.Error);
    }

    private static RegisterRequest CreateRegisterRequest() => new()
    {
        Email = "person@example.com",
        Password = "Password1!",
        FirstName = "Test",
        LastName = "Person"
    };

    private static Service CreateService(
        Mock<UserManager<ApplicationUser>> userManager,
        Mock<ITokenService>? tokenService = null,
        Mock<IUserProfileRepository>? userProfileRepository = null)
    {
        return new Service(
            userManager.Object,
            (userProfileRepository ??
                new Mock<IUserProfileRepository>()).Object,
            (tokenService ?? new Mock<ITokenService>()).Object);
    }

    private static Mock<UserManager<ApplicationUser>> CreateUserManager()
    {
        return new Mock<UserManager<ApplicationUser>>(
            Mock.Of<IUserStore<ApplicationUser>>(),
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!);
    }
}
