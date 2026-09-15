namespace Intertwine.Services.Constants;

/// <summary>
/// API messages for answer-submission idempotency conflicts.
/// </summary>
public static class IdempotencyMessages
{
    public const string KeyRequired = "An Idempotency-Key header is required.";
    public const string RequestInProgress = "An identical answer submission is already in progress.";
    public const string KeyUsedForDifferentRequest =
        "This Idempotency-Key has already been used for a different request.";
}
