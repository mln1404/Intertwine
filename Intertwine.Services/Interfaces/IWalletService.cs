using Intertwine.Services.DTOs.Wallets;

namespace Intertwine.Services.Interfaces;

/// <summary>
/// Provides wallet balances, demo top-ups, and payment history.
/// </summary>
public interface IWalletService
{
    /// <summary>Gets one page of payment history for the current user.</summary>
    Task<IReadOnlyList<PaymentHistoryDto>> GetPaymentsAsync(string identityUserId, int page = 1,
        CancellationToken cancellationToken = default);
    /// <summary>Gets the current user's Spark balance.</summary>
    Task<WalletDto> GetWalletAsync(
        string identityUserId,
        CancellationToken cancellationToken = default);

    /// <summary>Completes the current demo top-up flow for a selected package.</summary>
    Task<TopUpResultDto> TopUpAsync(
        string identityUserId,
        TopUpRequest request,
        CancellationToken cancellationToken = default);
}
