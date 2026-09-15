namespace Intertwine.Services.Interfaces.Repositories;

/// <summary>
/// Coordinates idempotent answer submissions across API instances.
/// </summary>
public interface IAnswerSubmissionIdempotencyStore
{
    /// <summary>
    /// Attempts to reserve an idempotency key for one answer-submission payload.
    /// </summary>
    Task<AnswerSubmissionIdempotencyStatus> TryAcquireAsync(
        string identityUserId,
        string idempotencyKey,
        string requestFingerprint,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Records a successfully completed submission for replay during the retention period.
    /// </summary>
    Task CompleteAsync(
        string identityUserId,
        string idempotencyKey,
        string requestFingerprint,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Releases a reservation when the submission fails validation or a business rule.
    /// </summary>
    Task ReleaseAsync(
        string identityUserId,
        string idempotencyKey,
        string requestFingerprint,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// The result of attempting to reserve an answer-submission idempotency key.
/// </summary>
public enum AnswerSubmissionIdempotencyStatus
{
    Acquired,
    InProgress,
    Completed,
    KeyUsedForDifferentRequest
}
