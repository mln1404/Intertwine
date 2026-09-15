using Intertwine.Domain.Entities;
using Intertwine.Domain.Enums;
using Intertwine.Services.DTOs.Wallets;
using Intertwine.Services.Interfaces;
using Intertwine.Services.Interfaces.Repositories;

namespace Intertwine.Services.Services;

public class WalletService : IWalletService
{
    private readonly IUserProfileRepository _userProfileRepository;
    private readonly IUserWalletRepository _walletRepository;
    private readonly ICreditPackageRepository _creditPackageRepository;
    private readonly IUserPaymentRepository _paymentRepository;
    private readonly IFinancialTransactionRepository
        _financialTransactionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public WalletService(
        IUserProfileRepository userProfileRepository,
        IUserWalletRepository walletRepository,
        ICreditPackageRepository creditPackageRepository,
        IUserPaymentRepository paymentRepository,
        IFinancialTransactionRepository financialTransactionRepository,
        IUnitOfWork unitOfWork)
    {
        _userProfileRepository = userProfileRepository;
        _walletRepository = walletRepository;
        _creditPackageRepository = creditPackageRepository;
        _paymentRepository = paymentRepository;
        _financialTransactionRepository =
            financialTransactionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<WalletDto> GetWalletAsync(
        string identityUserId,
        CancellationToken cancellationToken = default)
    {
        var profile =
            await GetUserProfileAsync(identityUserId);

        var wallet =
            await _walletRepository.GetByUserProfileIdAsync(
                profile.UserProfileId,
                cancellationToken);

        return new WalletDto
        {
            CreditBalance = wallet?.CreditBalance ?? 0
        };
    }

    public async Task<TopUpResultDto> TopUpAsync(
        string identityUserId,
        TopUpRequest request,
        CancellationToken cancellationToken = default)
    {
        var profile =
            await GetUserProfileAsync(identityUserId);

        var package =
            await _creditPackageRepository.GetByIdAsync(
                request.CreditPackageId,
                cancellationToken);

        if (package == null)
        {
            throw new InvalidOperationException(
                "Credit package not found or unavailable.");
        }

        var wallet =
            await GetOrCreateWalletAsync(
                profile.UserProfileId,
                identityUserId,
                cancellationToken);

        var payment = CreatePayment(
            profile.UserProfileId,
            package,
            identityUserId);

        await _paymentRepository.AddAsync(
            payment,
            cancellationToken);

        wallet.CreditBalance += package.Credits;
        wallet.DateUpdated = DateTime.UtcNow;
        wallet.UpdatedBy = identityUserId;

        var transaction = new FinancialTransaction
        {
            UserWallet = wallet,
            UserPayment = payment,

            CreditAmount = package.Credits,
            BalanceAfterTransaction = wallet.CreditBalance,

            TransactionType =
                FinancialTransactionType.CreditPurchase,

            Description = "Credit top-up",

            DateCreated = DateTime.UtcNow,
            CreatedBy = identityUserId
        };

        await _financialTransactionRepository.AddAsync(
            transaction,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return new TopUpResultDto
        {
            UserPaymentId = payment.UserPaymentId,
            CreditsPurchased = package.Credits,
            CreditBalance = wallet.CreditBalance
        };
    }

    private async Task<UserProfile> GetUserProfileAsync(
        string identityUserId)
    {
        var profile =
            await _userProfileRepository
                .GetByIdentityUserIdAsync(identityUserId);

        if (profile == null)
        {
            throw new InvalidOperationException(
                "User profile not found.");
        }

        return profile;
    }

    private async Task<UserWallet> GetOrCreateWalletAsync(
        int userProfileId,
        string identityUserId,
        CancellationToken cancellationToken)
    {
        var wallet =
            await _walletRepository.GetByUserProfileIdAsync(
                userProfileId,
                cancellationToken);

        if (wallet != null)
        {
            return wallet;
        }

        wallet = new UserWallet
        {
            UserProfileId = userProfileId,
            CreditBalance = 0,
            DateCreated = DateTime.UtcNow,
            CreatedBy = identityUserId
        };

        await _walletRepository.AddAsync(
            wallet,
            cancellationToken);

        return wallet;
    }

    private static UserPayment CreatePayment(
        int userProfileId,
        CreditPackage package,
        string identityUserId)
    {
        return new UserPayment
        {
            UserProfileId = userProfileId,

            // Snapshot the purchase.
            CurrencyCode = package.CurrencyCode,
            Amount = package.Amount,
            CreditsPurchased = package.Credits,

            // Mocked for MVP.
            PaymentProvider = "IntertwineDemo",
            Status = PaymentStatus.Completed,

            DateCreated = DateTime.UtcNow,
            CreatedBy = identityUserId
        };
    }
}
