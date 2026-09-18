namespace Intertwine.Domain.Enums;

/// <summary>
/// Represents the processing state of a user payment.
/// </summary>
public enum PaymentStatus
{
    Pending = 1,
    Completed = 2,
    Failed = 3,
    Cancelled = 4,
    Refunded = 5
}
