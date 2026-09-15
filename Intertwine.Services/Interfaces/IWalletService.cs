using Intertwine.Services.DTOs.Wallets;

namespace Intertwine.Services.Interfaces;

public interface IWalletService
{
    Task<WalletDto> GetWalletAsync(
        string identityUserId,
        CancellationToken cancellationToken = default);

    Task<TopUpResultDto> TopUpAsync(
        string identityUserId,
        TopUpRequest request,
        CancellationToken cancellationToken = default);
}