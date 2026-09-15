using System.Text.Json;
using Intertwine.Services.DTOs.Questions;
using Intertwine.Services.Interfaces.Repositories;
using StackExchange.Redis;

namespace Intertwine.Repositories.Caching;

/// <summary>
/// Stores Daily Question response data in Redis for one day per local date.
/// </summary>
public class RedisDailyQuestionCache : IDailyQuestionCache
{
    private const string KeyPrefix = "intertwine:daily-question:";
    private static readonly TimeSpan Expiration = TimeSpan.FromHours(24);

    private readonly IConnectionMultiplexer _connectionMultiplexer;

    public RedisDailyQuestionCache(IConnectionMultiplexer connectionMultiplexer)
    {
        _connectionMultiplexer = connectionMultiplexer;
    }

    public async Task<QuestionDetailDto?> GetAsync(
        DateOnly localDate,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var value = await _connectionMultiplexer
            .GetDatabase()
            .StringGetAsync(CreateKey(localDate));

        return value.IsNullOrEmpty
            ? null
            : JsonSerializer.Deserialize<QuestionDetailDto>(value.ToString());
    }

    public async Task SetAsync(
        DateOnly localDate,
        QuestionDetailDto question,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var serializedQuestion = JsonSerializer.Serialize(question);

        await _connectionMultiplexer
            .GetDatabase()
            .StringSetAsync(
                CreateKey(localDate),
                serializedQuestion,
                Expiration);
    }

    private static string CreateKey(DateOnly localDate) =>
        $"{KeyPrefix}{localDate:yyyy-MM-dd}";

    public async Task RemoveAsync(DateOnly localDate, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await _connectionMultiplexer.GetDatabase().KeyDeleteAsync(CreateKey(localDate));
    }
}
