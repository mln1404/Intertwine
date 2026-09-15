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

    public async Task<IReadOnlyList<PaymentHistoryDto>> GetPaymentsAsync(string identityUserId, int page = 1,
        CancellationToken cancellationToken = default)
    {
        if (page < 1 || page > 100000) throw new ArgumentOutOfRangeException(nameof(page));
        var profile = await GetUserProfileAsync(identityUserId);
        var payments = await _paymentRepository.GetByUserProfileIdAsync(profile.UserProfileId, (page - 1) * 20, 20, cancellationToken);
        return payments.Select(x => new PaymentHistoryDto
        {
            UserPaymentId = x.UserPaymentId, DateCreated = x.DateCreated,
            CurrencyCode = x.CurrencyCode, Amount = x.Amount,
            SparksPurchased = x.CreditsPurchased, Status = x.Status.ToString(), PaymentProvider = x.PaymentProvider
        }).ToList();
    }
    public async Task<TopUpResultDto> TopUpAsync(
    string identityUserId,
    TopUpRequest request,
    CancellationToken cancellationToken = default)
    {
        var profile =
            await GetUserProfileAsync(
                identityUserId);

        var package =
            await GetCreditPackageAsync(
                request.CreditPackageId,
                cancellationToken);

        var wallet =
            await GetOrCreateWalletAsync(
                profile.UserProfileId,
                identityUserId,
                cancellationToken);

        var payment = CreatePayment(
            profile.UserProfileId,
            package,
            identityUserId);

        AddCreditsToWallet(
            wallet,
            package.Credits,
            identityUserId);

        var transaction = CreateFinancialTransaction(
            wallet,
            payment,
            package.Credits,
            identityUserId);

        await _paymentRepository.AddAsync(
            payment,
            cancellationToken);

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
    private async Task<CreditPackage> GetCreditPackageAsync(
    int creditPackageId,
    CancellationToken cancellationToken)
    {
        var package =
            await _creditPackageRepository.GetByIdAsync(
                creditPackageId,
                cancellationToken);

        if (package == null)
        {
            throw new InvalidOperationException(
                "Credit package not found or unavailable.");
        }

        return package;
    }

    private static void AddCreditsToWallet(
        UserWallet wallet,
        long credits,
        string identityUserId)
    {
        wallet.CreditBalance += credits;
        wallet.DateUpdated = DateTime.UtcNow;
        wallet.UpdatedBy = identityUserId;
    }

    private static FinancialTransaction CreateFinancialTransaction(
        UserWallet wallet,
        UserPayment payment,
        long credits,
        string identityUserId)
    {
        return new FinancialTransaction
        {
            UserWallet = wallet,
            UserPayment = payment,

            CreditAmount = credits,
            BalanceAfterTransaction = wallet.CreditBalance,

            TransactionType =
                FinancialTransactionType.CreditPurchase,

            Description = "Credit top-up",

            DateCreated = DateTime.UtcNow,
            CreatedBy = identityUserId
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
