namespace Intertwine.Services.Interfaces.Repositories;

/// <summary>
/// Provides persistence access to a user's Spark wallet.
/// </summary>
public interface IUserWalletRepository
{
    /// <summary>Gets the wallet linked to a profile.</summary>
    Task<UserWallet?> GetByUserProfileIdAsync(
        int userProfileId,
        CancellationToken cancellationToken = default);

    /// <summary>Adds a wallet to the current unit of work.</summary>
    Task AddAsync(
        UserWallet wallet,
        CancellationToken cancellationToken = default);
}
