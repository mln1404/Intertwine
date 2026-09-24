using Intertwine.Services.DTOs.Discovery;
using Intertwine.Services.Interfaces.Repositories;
using Intertwine.Services.Services;
using Moq;

namespace Intertwine.UnitTests.Services.DailyQuestionDiscoveryService;

public class DailyQuestionDiscoveryServiceTests
{
    private static readonly DateOnly Today = new(2026, 9, 24);

    [Fact]
    public async Task GetUsersAsync_SameAnswer_ReturnsPagedUsersAndExcludesRequesterThroughQuery()
    {
        var repository = CreateRepository(CreateContext(Today, currentAnswerId: 10));
        repository.Setup(x => x.GetUsersAsync(
                10,
                "requester",
                1,
                20,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DiscoveryUsersPageData(
                21,
                [new DiscoveryUserDto
                {
                    UserProfileId = 2,
                    AvatarName = "Merrick Lance",
                    PersonalityTypeCode = null
                }]));
        var service = CreateService(repository);

        var result = await service.GetUsersAsync(
            "requester", 5, 10, Today, 1, 20);

        Assert.NotNull(result);
        Assert.Equal(DiscoveryAccessState.Included, result.AccessState);
        Assert.Equal(21, result.TotalCount);
        Assert.True(result.HasMore);
        var user = Assert.Single(result.Users);
        Assert.Equal("Merrick Lance", user.AvatarName);
        Assert.Null(user.PersonalityTypeCode);
        repository.Verify(x => x.GetUsersAsync(
            10,
            "requester",
            1,
            20,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetContextAsync_WhenUserHasNotAnswered_RejectsDiscovery()
    {
        var service = CreateService(CreateRepository(CreateContext(Today, null)));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.GetContextAsync("requester", 5, Today));
    }

    [Fact]
    public async Task GetContextAsync_WhenDailyQuestionIsInvalid_ReturnsNull()
    {
        var service = CreateService(CreateRepository(null));

        Assert.Null(await service.GetContextAsync("requester", 999, Today));
    }

    [Fact]
    public async Task GetUsersAsync_WhenAnswerDoesNotBelong_RejectsRequest()
    {
        var service = CreateService(CreateRepository(CreateContext(Today, 10)));

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.GetUsersAsync("requester", 5, 999, Today, 1, 20));
    }

    [Fact]
    public async Task GetUsersAsync_DifferentAnswer_ReturnsSparksRequiredWithoutQueryingUsers()
    {
        var repository = CreateRepository(CreateContext(Today, 10));
        var service = CreateService(repository);

        var result = await service.GetUsersAsync(
            "requester", 5, 11, Today, 1, 20);

        Assert.NotNull(result);
        Assert.Equal(DiscoveryAccessState.SparksRequired, result.AccessState);
        Assert.Empty(result.Users);
        repository.Verify(x => x.GetUsersAsync(
            It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetUsersAsync_SubscriptionDay_ReturnsSubscriptionRequired()
    {
        var service = CreateService(CreateRepository(CreateContext(Today.AddDays(-3), 10)));

        var result = await service.GetUsersAsync(
            "requester", 5, 10, Today, 1, 20);

        Assert.NotNull(result);
        Assert.Equal(DiscoveryAccessState.SubscriptionRequired, result.AccessState);
    }

    [Fact]
    public async Task GetUsersAsync_OutsideWindow_ReturnsUnavailable()
    {
        var service = CreateService(CreateRepository(CreateContext(Today.AddDays(-8), 10)));

        var result = await service.GetUsersAsync(
            "requester", 5, 10, Today, 1, 20);

        Assert.NotNull(result);
        Assert.Equal(DiscoveryAccessState.Unavailable, result.AccessState);
    }

    [Fact]
    public async Task GetContextAsync_UsesCurrentAnswerToMoveIncludedPoolImmediately()
    {
        var service = CreateService(CreateRepository(CreateContext(Today, 11)));

        var result = await service.GetContextAsync("requester", 5, Today);

        Assert.NotNull(result);
        Assert.Equal(11, result.CurrentUserAnswerId);
        Assert.Equal(
            DiscoveryAccessState.Included,
            result.AnswerPools.Single(x => x.AnswerId == 11).AccessState);
        Assert.Equal(
            DiscoveryAccessState.SparksRequired,
            result.AnswerPools.Single(x => x.AnswerId == 10).AccessState);
    }

    [Fact]
    public void DiscoveryUserDto_DoesNotExposePrivateIdentityFields()
    {
        var properties = typeof(DiscoveryUserDto).GetProperties()
            .Select(x => x.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        Assert.DoesNotContain("FirstName", properties);
        Assert.DoesNotContain("LastName", properties);
        Assert.DoesNotContain("Email", properties);
        Assert.DoesNotContain("IdentityUserId", properties);
    }

    private static Mock<IDailyQuestionDiscoveryRepository> CreateRepository(
        DiscoveryContextData? context)
    {
        var repository = new Mock<IDailyQuestionDiscoveryRepository>();
        repository.Setup(x => x.GetContextAsync(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(context);
        repository.Setup(x => x.GetHistoryAsync(
                It.IsAny<DateOnly>(),
                It.IsAny<DateOnly>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        return repository;
    }

    private static Intertwine.Services.Services.DailyQuestionDiscoveryService CreateService(
        Mock<IDailyQuestionDiscoveryRepository> repository) =>
        new(repository.Object, new DailyQuestionDiscoveryAccessPolicy());

    private static DiscoveryContextData CreateContext(
        DateOnly date,
        int? currentAnswerId) =>
        new(
            5,
            date,
            7,
            "Connection",
            "What matters?",
            currentAnswerId,
            currentAnswerId == 10 ? "Kindness" : currentAnswerId == 11 ? "Humor" : null,
            [
                new DiscoveryAnswerData(10, "Kindness"),
                new DiscoveryAnswerData(11, "Humor")
            ]);
}
