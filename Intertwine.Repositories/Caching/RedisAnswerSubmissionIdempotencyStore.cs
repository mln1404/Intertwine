using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Intertwine.Services.Interfaces.Repositories;
using StackExchange.Redis;

namespace Intertwine.Repositories.Caching;

/// <summary>
/// Redis-backed coordination for idempotent answer-submission requests.
/// </summary>
public class RedisAnswerSubmissionIdempotencyStore
    : IAnswerSubmissionIdempotencyStore
{
    private static readonly TimeSpan ProcessingLifetime = TimeSpan.FromMinutes(2);
    private static readonly TimeSpan CompletedLifetime = TimeSpan.FromHours(24);

    private readonly IConnectionMultiplexer _connectionMultiplexer;

    public RedisAnswerSubmissionIdempotencyStore(
        IConnectionMultiplexer connectionMultiplexer)
    {
        _connectionMultiplexer = connectionMultiplexer;
    }

    /// <inheritdoc />
    public async Task<AnswerSubmissionIdempotencyStatus> TryAcquireAsync(
        string identityUserId,
        string idempotencyKey,
        string requestFingerprint,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var database = _connectionMultiplexer.GetDatabase();
        var redisKey = CreateRedisKey(identityUserId, idempotencyKey);
        var existing = await database.StringGetAsync(redisKey);

        if (existing.HasValue)
        {
            return GetExistingStatus(existing!, requestFingerprint);
        }

        var processingRecord = new IdempotencyRecord(
            requestFingerprint,
            AnswerSubmissionIdempotencyStatus.InProgress);
        var wasCreated = await database.StringSetAsync(
            redisKey,
            JsonSerializer.Serialize(processingRecord),
            ProcessingLifetime,
            When.NotExists);

        if (wasCreated)
        {
            return AnswerSubmissionIdempotencyStatus.Acquired;
        }

        existing = await database.StringGetAsync(redisKey);
        return existing.HasValue
            ? GetExistingStatus(existing!, requestFingerprint)
            : AnswerSubmissionIdempotencyStatus.InProgress;
    }

    /// <inheritdoc />
    public async Task CompleteAsync(
        string identityUserId,
        string idempotencyKey,
        string requestFingerprint,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var completedRecord = new IdempotencyRecord(
            requestFingerprint,
            AnswerSubmissionIdempotencyStatus.Completed);

        await _connectionMultiplexer
            .GetDatabase()
            .StringSetAsync(
                CreateRedisKey(identityUserId, idempotencyKey),
                JsonSerializer.Serialize(completedRecord),
                CompletedLifetime);
    }

    /// <inheritdoc />
    public async Task ReleaseAsync(
        string identityUserId,
        string idempotencyKey,
        string requestFingerprint,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var database = _connectionMultiplexer.GetDatabase();
        var redisKey = CreateRedisKey(identityUserId, idempotencyKey);
        var existing = await database.StringGetAsync(redisKey);

        if (!existing.HasValue)
        {
            return;
        }

        var record = Deserialize(existing!);
        if (record is { Status: AnswerSubmissionIdempotencyStatus.InProgress }
            && record.RequestFingerprint == requestFingerprint)
        {
            await database.KeyDeleteAsync(redisKey);
        }
    }

    private static AnswerSubmissionIdempotencyStatus GetExistingStatus(
        RedisValue value,
        string requestFingerprint)
    {
        var record = Deserialize(value);

        if (record is null || record.RequestFingerprint != requestFingerprint)
        {
            return AnswerSubmissionIdempotencyStatus.KeyUsedForDifferentRequest;
        }

        return record.Status;
    }

    private static IdempotencyRecord? Deserialize(RedisValue value) =>
        JsonSerializer.Deserialize<IdempotencyRecord>(value.ToString());

    private static RedisKey CreateRedisKey(
        string identityUserId,
        string idempotencyKey)
    {
        var keyMaterial = $"{identityUserId}:{idempotencyKey}";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(keyMaterial));

        return $"intertwine:answer-submission:idempotency:{Convert.ToHexString(hash)}";
    }

    private sealed record IdempotencyRecord(
        string RequestFingerprint,
        AnswerSubmissionIdempotencyStatus Status);
}
