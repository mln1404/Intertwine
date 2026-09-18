namespace Intertwine.Services.Interfaces.Repositories;

/// <summary>
/// Provides persistence access to a user's payment records.
/// </summary>
public interface IUserPaymentRepository
{
    /// <summary>Gets one page of payments ordered for display.</summary>
    Task<IReadOnlyList<UserPayment>> GetByUserProfileIdAsync(int userProfileId, int skip, int take,
        CancellationToken cancellationToken = default);

    /// <summary>Adds a payment record to the current unit of work.</summary>
    Task AddAsync(
        UserPayment payment,
        CancellationToken cancellationToken = default);
}
