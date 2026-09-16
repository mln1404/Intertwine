using Intertwine.Repositories.Caching;
using Intertwine.Services.DTOs.Questions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using StackExchange.Redis;

namespace Intertwine.UnitTests.Caching;

public class RedisDailyQuestionCacheTests
{
    private static readonly DateOnly LocalDate = new(2026, 9, 16);

    [Fact]
    public async Task GetAsync_WhenRedisIsDisconnected_ReturnsCacheMissWithoutWaitingForRedis()
    {
        var connection = CreateDisconnectedConnection();
        var cache = CreateCache(connection);

        var result = await cache.GetAsync(LocalDate);

        Assert.Null(result);
        connection.Verify(
            x => x.GetDatabase(It.IsAny<int>(), It.IsAny<object>()),
            Times.Never);
    }

    [Fact]
    public async Task SetAndRemoveAsync_WhenRedisIsDisconnected_CompleteWithoutUsingRedis()
    {
        var connection = CreateDisconnectedConnection();
        var cache = CreateCache(connection);

        await cache.SetAsync(LocalDate, new QuestionDetailDto());
        await cache.RemoveAsync(LocalDate);

        connection.Verify(
            x => x.GetDatabase(It.IsAny<int>(), It.IsAny<object>()),
            Times.Never);
    }

    private static RedisDailyQuestionCache CreateCache(
        Mock<IConnectionMultiplexer> connection) =>
        new(
            connection.Object,
            NullLogger<RedisDailyQuestionCache>.Instance);

    private static Mock<IConnectionMultiplexer> CreateDisconnectedConnection()
    {
        var connection = new Mock<IConnectionMultiplexer>(MockBehavior.Strict);
        connection.SetupGet(x => x.IsConnected).Returns(false);
        return connection;
    }
}
