using Intertwine.Domain.Entities;
using Intertwine.Services.Constants;
using Intertwine.Services.Interfaces.Repositories;
using Moq;
using Service = Intertwine.Services.Services.UserDailyActivityService;

namespace Intertwine.UnitTests.Services.UserDailyActivityService;

public class UserDailyActivityServiceTests
{
    private const string IdentityUserId = "identity-1";
    private const int UserProfileId = 1;
    private static readonly DateOnly LocalDate = new(2026, 9, 16);

    [Fact]
    public async Task GetCurrentUserActivityAsync_WhenNoActivityExists_ReturnsFullAllowance()
    {
        var userProfiles = CreateUserProfileRepository();
        var activities = new Mock<IUserDailyActivityRepository>();
        activities.Setup(x => x.GetByUserAndDateAsync(
                UserProfileId,
                LocalDate,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserDailyActivity?)null);
        var service = new Service(userProfiles.Object, activities.Object);

        var result = await service.GetCurrentUserActivityAsync(
            IdentityUserId,
            LocalDate);

        Assert.Equal(LocalDate, result.LocalDate);
        Assert.False(result.DailyQuestionCreateOrUpdateUsed);
        Assert.Equal(0, result.NonDailyQuestionsAnswered);
        Assert.Equal(
            UserAnswerLimits.MaxNonDailyQuestionsPerDay,
            result.NonDailyQuestionsRemaining);
    }

    [Fact]
    public async Task GetCurrentUserActivityAsync_WhenActivityExists_MapsUsage()
    {
        var userProfiles = CreateUserProfileRepository();
        var activities = new Mock<IUserDailyActivityRepository>();
        activities.Setup(x => x.GetByUserAndDateAsync(
                UserProfileId,
                LocalDate,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserDailyActivity
            {
                UserProfileId = UserProfileId,
                Date = LocalDate,
                DailyQuestionCreateOrUpdateUsed = true,
                NonDailyQuestionsAnswered = 1
            });
        var service = new Service(userProfiles.Object, activities.Object);

        var result = await service.GetCurrentUserActivityAsync(
            IdentityUserId,
            LocalDate);

        Assert.True(result.DailyQuestionCreateOrUpdateUsed);
        Assert.Equal(1, result.NonDailyQuestionsAnswered);
        Assert.Equal(1, result.NonDailyQuestionsRemaining);
    }

    [Fact]
    public async Task GetCurrentUserActivityAsync_WhenProfileDoesNotExist_Throws()
    {
        var userProfiles = new Mock<IUserProfileRepository>();
        userProfiles.Setup(x => x.GetByIdentityUserIdAsync(IdentityUserId))
            .ReturnsAsync((UserProfile?)null);
        var activities = new Mock<IUserDailyActivityRepository>();
        var service = new Service(userProfiles.Object, activities.Object);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.GetCurrentUserActivityAsync(
                IdentityUserId,
                LocalDate));

        Assert.Equal(UserAnswerMessages.UserProfileNotFound, exception.Message);
        activities.Verify(x => x.GetByUserAndDateAsync(
                It.IsAny<int>(),
                It.IsAny<DateOnly>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private static Mock<IUserProfileRepository> CreateUserProfileRepository()
    {
        var repository = new Mock<IUserProfileRepository>();
        repository.Setup(x => x.GetByIdentityUserIdAsync(IdentityUserId))
            .ReturnsAsync(new UserProfile
            {
                UserProfileId = UserProfileId,
                IdentityUserId = IdentityUserId
            });
        return repository;
    }
}
