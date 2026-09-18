using Intertwine.Domain.Entities;
using Intertwine.Identity;
using Intertwine.Services.Constants;
using Intertwine.Services.DTOs.Authentication;
using Intertwine.Services.Interfaces;
using Intertwine.Services.Interfaces.Repositories;
using Microsoft.AspNetCore.Identity;
using Moq;
using Microsoft.Extensions.Options;
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
    public async Task RegisterAsync_WhenIdentityCreationSucceeds_CreatesProfileAndWallet()
    {
        var userManager = CreateUserManager();
        var profiles = new Mock<IUserProfileRepository>();
        userManager.Setup(x => x.FindByEmailAsync("person@example.com"))
            .ReturnsAsync((ApplicationUser?)null);
        userManager.Setup(x => x.CreateAsync(
                It.IsAny<ApplicationUser>(),
                "Password1!"))
            .Callback<ApplicationUser, string>((user, _) => user.Id = "identity-1")
            .ReturnsAsync(IdentityResult.Success);
        profiles.Setup(x => x.AddAsync(It.IsAny<UserProfile>()))
            .ReturnsAsync((UserProfile profile) => profile);
        var service = CreateService(userManager, userProfileRepository: profiles);

        var result = await service.RegisterAsync(CreateRegisterRequest());

        Assert.True(result.Succeeded);
        Assert.Equal("identity-1", result.UserId);
        profiles.Verify(x => x.AddAsync(
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
            x => x.GenerateAccessToken(It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task LoginAsync_WhenCredentialsAreValid_ReturnsGeneratedToken()
    {
        var user = CreateIdentityUser();
        var userManager = CreateValidLoginUserManager(user);
        var tokenService = new Mock<ITokenService>(MockBehavior.Strict);
        tokenService.Setup(x => x.GenerateAccessToken(user.Id, user.Email!))
            .Returns("signed-token");
        tokenService.Setup(x => x.GenerateRefreshToken()).Returns("raw-refresh-token");
        tokenService.Setup(x => x.HashRefreshToken("raw-refresh-token"))
            .Returns("hashed-refresh-token");
        var service = CreateService(userManager, tokenService);

        var result = await service.LoginAsync(CreateLoginRequest());

        Assert.True(result.Succeeded);
        Assert.Equal(user.Id, result.UserId);
        Assert.Equal("signed-token", result.Token);
        Assert.Equal("raw-refresh-token", result.RefreshToken);
    }

    [Fact]
    public async Task LoginAsync_WhenProfileIsInactive_ReactivatesBeforeReturningToken()
    {
        var user = CreateIdentityUser();
        var userManager = CreateValidLoginUserManager(user);
        var profile = new UserProfile
        {
            IdentityUserId = user.Id,
            IsActive = false
        };
        var profiles = new Mock<IUserProfileRepository>(MockBehavior.Strict);
        profiles.Setup(x => x.GetByIdentityUserIdIncludingInactiveAsync(user.Id))
            .ReturnsAsync(profile);
        profiles.Setup(x => x.SetIsActiveAsync(profile, true, user.Id))
            .Callback(() => profile.IsActive = true)
            .Returns(Task.CompletedTask);
        var tokenService = new Mock<ITokenService>(MockBehavior.Strict);
        tokenService.Setup(x => x.GenerateAccessToken(user.Id, user.Email!))
            .Returns("signed-token");
        tokenService.Setup(x => x.GenerateRefreshToken()).Returns("raw-refresh-token");
        tokenService.Setup(x => x.HashRefreshToken("raw-refresh-token"))
            .Returns("hashed-refresh-token");
        var service = CreateService(userManager, tokenService, profiles);

        var result = await service.LoginAsync(CreateLoginRequest());

        Assert.True(result.Succeeded);
        Assert.True(profile.IsActive);
        profiles.Verify(x => x.SetIsActiveAsync(profile, true, user.Id), Times.Once);
        Assert.Equal("raw-refresh-token", result.RefreshToken);
    }

    [Fact]
    public async Task LogoutAsync_WithActiveToken_RevokesAndSavesOnce()
    {
        var token = new RefreshToken { TokenHash = "hashed-token" };
        var tokens = new Mock<IRefreshTokenRepository>(MockBehavior.Strict);
        tokens.Setup(x => x.GetByHashAsync("hashed-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(token);
        var tokenService = new Mock<ITokenService>(MockBehavior.Strict);
        tokenService.Setup(x => x.HashRefreshToken("raw-token")).Returns("hashed-token");
        var unitOfWork = new Mock<IUnitOfWork>();
        var service = CreateService(CreateUserManager(), tokenService,
            refreshTokenRepository: tokens, unitOfWork: unitOfWork);

        var before = DateTime.UtcNow;
        await service.LogoutAsync("raw-token");

        Assert.InRange(token.RevokedAtUtc!.Value, before, DateTime.UtcNow);
        tokens.Verify(x => x.GetByHashAsync("hashed-token", It.IsAny<CancellationToken>()), Times.Once);
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task LogoutAsync_WithoutCookie_DoesNothing(string? rawToken)
    {
        var tokenService = new Mock<ITokenService>(MockBehavior.Strict);
        var tokens = new Mock<IRefreshTokenRepository>(MockBehavior.Strict);
        var unitOfWork = new Mock<IUnitOfWork>();
        var service = CreateService(CreateUserManager(), tokenService,
            refreshTokenRepository: tokens, unitOfWork: unitOfWork);

        await service.LogoutAsync(rawToken);

        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task LogoutAsync_WithUnknownToken_DoesNotSave()
    {
        var tokens = new Mock<IRefreshTokenRepository>(MockBehavior.Strict);
        tokens.Setup(x => x.GetByHashAsync("unknown-hash", It.IsAny<CancellationToken>()))
            .ReturnsAsync((RefreshToken?)null);
        var tokenService = new Mock<ITokenService>(MockBehavior.Strict);
        tokenService.Setup(x => x.HashRefreshToken("unknown-token")).Returns("unknown-hash");
        var unitOfWork = new Mock<IUnitOfWork>();
        var service = CreateService(CreateUserManager(), tokenService,
            refreshTokenRepository: tokens, unitOfWork: unitOfWork);

        await service.LogoutAsync("unknown-token");

        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task LogoutAsync_WithAlreadyRevokedToken_DoesNotSave()
    {
        var revokedAt = DateTime.UtcNow.AddMinutes(-1);
        var token = new RefreshToken { RevokedAtUtc = revokedAt };
        var tokens = new Mock<IRefreshTokenRepository>(MockBehavior.Strict);
        tokens.Setup(x => x.GetByHashAsync("hashed-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(token);
        var tokenService = new Mock<ITokenService>(MockBehavior.Strict);
        tokenService.Setup(x => x.HashRefreshToken("raw-token")).Returns("hashed-token");
        var unitOfWork = new Mock<IUnitOfWork>();
        var service = CreateService(CreateUserManager(), tokenService,
            refreshTokenRepository: tokens, unitOfWork: unitOfWork);

        await service.LogoutAsync("raw-token");

        Assert.Equal(revokedAt, token.RevokedAtUtc);
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task LogoutAsync_WithExpiredStoredToken_RevokesIt()
    {
        var token = new RefreshToken { ExpiresAtUtc = DateTime.UtcNow.AddDays(-1) };
        var tokens = new Mock<IRefreshTokenRepository>(MockBehavior.Strict);
        tokens.Setup(x => x.GetByHashAsync("hashed-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(token);
        var tokenService = new Mock<ITokenService>(MockBehavior.Strict);
        tokenService.Setup(x => x.HashRefreshToken("raw-token")).Returns("hashed-token");
        var unitOfWork = new Mock<IUnitOfWork>();
        var service = CreateService(CreateUserManager(), tokenService,
            refreshTokenRepository: tokens, unitOfWork: unitOfWork);

        await service.LogoutAsync("raw-token");

        Assert.NotNull(token.RevokedAtUtc);
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LogoutAsync_WhenDatabaseSaveFails_PropagatesFailure()
    {
        var tokens = new Mock<IRefreshTokenRepository>(MockBehavior.Strict);
        tokens.Setup(x => x.GetByHashAsync("hashed-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RefreshToken());
        var tokenService = new Mock<ITokenService>(MockBehavior.Strict);
        tokenService.Setup(x => x.HashRefreshToken("raw-token")).Returns("hashed-token");
        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database unavailable"));
        var service = CreateService(CreateUserManager(), tokenService,
            refreshTokenRepository: tokens, unitOfWork: unitOfWork);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.LogoutAsync("raw-token"));
    }

    private static RegisterRequest CreateRegisterRequest() => new()
    {
        Email = "person@example.com",
        Password = "Password1!",
        FirstName = "Test",
        LastName = "Person"
    };

    private static LoginRequest CreateLoginRequest() => new()
    {
        Email = "person@example.com",
        Password = "Password1!"
    };

    private static ApplicationUser CreateIdentityUser() => new()
    {
        Id = "identity-1",
        Email = "person@example.com"
    };

    private static Mock<UserManager<ApplicationUser>> CreateValidLoginUserManager(
        ApplicationUser user)
    {
        var userManager = CreateUserManager();
        userManager.Setup(x => x.FindByEmailAsync(user.Email!))
            .ReturnsAsync(user);
        userManager.Setup(x => x.CheckPasswordAsync(user, "Password1!"))
            .ReturnsAsync(true);
        return userManager;
    }

    private static Service CreateService(
        Mock<UserManager<ApplicationUser>> userManager,
        Mock<ITokenService>? tokenService = null,
        Mock<IUserProfileRepository>? userProfileRepository = null,
        Mock<IRefreshTokenRepository>? refreshTokenRepository = null,
        Mock<IUnitOfWork>? unitOfWork = null,
        JwtSettings? jwtSettings = null)
    {
        var refreshRepo = (refreshTokenRepository ?? new Mock<IRefreshTokenRepository>()).Object;

        var uowMock = unitOfWork ?? new Mock<IUnitOfWork>();
        if (unitOfWork == null)
            uowMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

        var options = Options.Create(jwtSettings ?? new JwtSettings { RefreshTokenExpiryDays = 7 });

        return new Service(
            userManager.Object,
            (userProfileRepository ?? new Mock<IUserProfileRepository>()).Object,
            (tokenService ?? new Mock<ITokenService>()).Object,
            refreshRepo,
            uowMock.Object,
            options);
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
