using System.Text.Json;
using Intertwine.Services.DTOs.Questions;
using Intertwine.Services.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Intertwine.Repositories.Caching;

/// <summary>
/// Stores Daily Question response data in Redis for one day per local date.
/// Cache failures are treated as misses so Daily Questions remain available from SQL.
/// </summary>
public class RedisDailyQuestionCache : IDailyQuestionCache
{
    private const string KeyPrefix = "intertwine:daily-question:v2:";
    private static readonly TimeSpan Expiration = TimeSpan.FromHours(24);

    private readonly IConnectionMultiplexer _connectionMultiplexer;
    private readonly ILogger<RedisDailyQuestionCache> _logger;

    public RedisDailyQuestionCache(
        IConnectionMultiplexer connectionMultiplexer,
        ILogger<RedisDailyQuestionCache> logger)
    {
        _connectionMultiplexer = connectionMultiplexer;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<QuestionDetailDto?> GetAsync(
        DateOnly localDate,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!IsRedisAvailable("read"))
            return null;

        try
        {
            var value = await _connectionMultiplexer
                .GetDatabase()
                .StringGetAsync(CreateKey(localDate));

            return value.IsNullOrEmpty
                ? null
                : JsonSerializer.Deserialize<QuestionDetailDto>(value.ToString());
        }
        catch (RedisException exception)
        {
            LogCacheFailure("read", exception);
            return null;
        }
    }

    /// <inheritdoc />
    public async Task SetAsync(
        DateOnly localDate,
        QuestionDetailDto question,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!IsRedisAvailable("write"))
            return;

        try
        {
            var serializedQuestion = JsonSerializer.Serialize(question);

            await _connectionMultiplexer
                .GetDatabase()
                .StringSetAsync(
                    CreateKey(localDate),
                    serializedQuestion,
                    Expiration);
        }
        catch (RedisException exception)
        {
            LogCacheFailure("write", exception);
        }
    }

    /// <inheritdoc />
    public async Task RemoveAsync(
        DateOnly localDate,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!IsRedisAvailable("remove"))
            return;

        try
        {
            await _connectionMultiplexer
                .GetDatabase()
                .KeyDeleteAsync(CreateKey(localDate));
        }
        catch (RedisException exception)
        {
            LogCacheFailure("remove", exception);
        }
    }

    private bool IsRedisAvailable(string operation)
    {
        if (_connectionMultiplexer.IsConnected)
            return true;

        _logger.LogWarning(
            "Redis is unavailable. Skipping Daily Question cache {Operation} and using the database path.",
            operation);
        return false;
    }

    private void LogCacheFailure(
        string operation,
        RedisException exception)
    {
        _logger.LogWarning(
            exception,
            "Daily Question cache {Operation} failed. Continuing without cached data.",
            operation);
    }

    private static string CreateKey(DateOnly localDate) =>
        $"{KeyPrefix}{localDate:yyyy-MM-dd}";
}
