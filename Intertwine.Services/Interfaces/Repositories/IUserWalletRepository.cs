namespace Intertwine.Services.Interfaces.Repositories;

public interface IUserWalletRepository
{
    Task<UserWallet?> GetByUserProfileIdAsync(
        int userProfileId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        UserWallet wallet,
        CancellationToken cancellationToken = default);
}
