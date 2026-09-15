using Intertwine.Services.DTOs.Wallets;

namespace Intertwine.Services.Interfaces;

public interface IWalletService
{
    Task<IReadOnlyList<PaymentHistoryDto>> GetPaymentsAsync(string identityUserId, int page = 1,
        CancellationToken cancellationToken = default);
    Task<WalletDto> GetWalletAsync(
        string identityUserId,
        CancellationToken cancellationToken = default);

    Task<TopUpResultDto> TopUpAsync(
        string identityUserId,
        TopUpRequest request,
        CancellationToken cancellationToken = default);
}
