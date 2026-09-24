using Intertwine.Services.DTOs.Discovery;
using Intertwine.Services.Services;

namespace Intertwine.UnitTests.Services.DailyQuestionDiscoveryService;

public class DailyQuestionDiscoveryAccessPolicyTests
{
    private static readonly DateOnly Today = new(2026, 9, 24);
    private readonly DailyQuestionDiscoveryAccessPolicy _policy = new();

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public void GetDateAccess_TodayAndYesterday_AreIncluded(int daysAgo)
    {
        Assert.Equal(
            DiscoveryAccessState.Included,
            _policy.GetDateAccess(Today.AddDays(-daysAgo), Today));
    }

    [Theory]
    [InlineData(2)]
    [InlineData(7)]
    public void GetDateAccess_TwoThroughSevenDaysAgo_RequiresSubscription(int daysAgo)
    {
        Assert.Equal(
            DiscoveryAccessState.SubscriptionRequired,
            _policy.GetDateAccess(Today.AddDays(-daysAgo), Today));
    }

    [Theory]
    [InlineData(8)]
    [InlineData(-1)]
    public void GetDateAccess_OutsideWindow_IsUnavailable(int daysAgo)
    {
        Assert.Equal(
            DiscoveryAccessState.Unavailable,
            _policy.GetDateAccess(Today.AddDays(-daysAgo), Today));
    }

    [Fact]
    public void GetAnswerPoolAccess_DifferentAnswer_RequiresSparks()
    {
        Assert.Equal(
            DiscoveryAccessState.SparksRequired,
            _policy.GetAnswerPoolAccess(DiscoveryAccessState.Included, 10, 11));
    }
}
