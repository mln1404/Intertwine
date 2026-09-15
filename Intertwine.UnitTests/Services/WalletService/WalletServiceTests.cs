using Intertwine.Domain.Entities;
using Intertwine.Domain.Enums;
using Intertwine.Services.DTOs.Wallets;
using Intertwine.Services.Interfaces.Repositories;
using Moq;
using Service = Intertwine.Services.Services.WalletService;

namespace Intertwine.UnitTests.Services.WalletService;

public class WalletServiceTests
{
    [Fact]
    public async Task GetPayments_UsesAuthenticatedProfileAndPagesPurchaseSnapshots()
    {
        var context = CreateContext();
        context.Payments.Setup(x => x.GetByUserProfileIdAsync(12, 20, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync([new UserPayment { UserPaymentId = 99, CreditsPurchased = 1000, Amount = 9.99m, CurrencyCode = "AUD", Status = PaymentStatus.Completed, PaymentProvider = "IntertwineDemo" }]);
        var result = await context.Service.GetPaymentsAsync("identity-12", 2);
        var payment = Assert.Single(result);
        Assert.Equal(1000, payment.SparksPurchased);
        Assert.Equal("Completed", payment.Status);
        Assert.Equal(9.99m, payment.Amount);
        context.Payments.Verify(x => x.GetByUserProfileIdAsync(12, 20, 20, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetPayments_MissingProfileCannotReadHistory()
    {
        var context = CreateContext(false);
        await Assert.ThrowsAsync<InvalidOperationException>(() => context.Service.GetPaymentsAsync("unknown"));
        context.Payments.Verify(x => x.GetByUserProfileIdAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetWalletAsync_WhenWalletExists_ReturnsCurrentBalance()
    {
        var context = CreateContext();
        context.Wallets.Setup(x => x.GetByUserProfileIdAsync(
                context.Profile.UserProfileId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserWallet { CreditBalance = 750 });

        var result = await context.Service.GetWalletAsync(
            context.Profile.IdentityUserId);

        Assert.Equal(750, result.CreditBalance);
    }

    [Fact]
    public async Task GetWalletAsync_WhenWalletDoesNotExist_ReturnsZeroBalance()
    {
        var context = CreateContext();
        context.Wallets.Setup(x => x.GetByUserProfileIdAsync(
                context.Profile.UserProfileId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserWallet?)null);

        var result = await context.Service.GetWalletAsync(
            context.Profile.IdentityUserId);

        Assert.Equal(0, result.CreditBalance);
    }

    [Fact]
    public async Task GetWalletAsync_WhenProfileDoesNotExist_ThrowsWithoutQueryingWallet()
    {
        var context = CreateContext(profileExists: false);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            context.Service.GetWalletAsync("missing-user"));

        context.Wallets.Verify(
            x => x.GetByUserProfileIdAsync(
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task TopUpAsync_WithExistingWallet_AddsCreditsAndStagesAuditRecords()
    {
        var context = CreateContext();
        var wallet = new UserWallet
        {
            UserWalletId = 8,
            UserProfileId = context.Profile.UserProfileId,
            CreditBalance = 250
        };
        SetupSuccessfulTopUp(context, wallet);

        var result = await context.Service.TopUpAsync(
            context.Profile.IdentityUserId,
            new TopUpRequest { CreditPackageId = context.Package.CreditPackageId });

        Assert.Equal(1_000, result.CreditsPurchased);
        Assert.Equal(1_250, result.CreditBalance);
        Assert.Equal(1_250, wallet.CreditBalance);
        Assert.Equal(context.Profile.IdentityUserId, wallet.UpdatedBy);
        Assert.NotNull(wallet.DateUpdated);
        context.Wallets.Verify(
            x => x.AddAsync(
                It.IsAny<UserWallet>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
        VerifySuccessfulTopUp(context, expectedBalance: 1_250);
    }

    [Fact]
    public async Task TopUpAsync_WithoutWallet_CreatesWalletAndStagesAuditRecords()
    {
        var context = CreateContext();
        SetupSuccessfulTopUp(context, wallet: null);

        var result = await context.Service.TopUpAsync(
            context.Profile.IdentityUserId,
            new TopUpRequest { CreditPackageId = context.Package.CreditPackageId });

        Assert.Equal(1_000, result.CreditBalance);
        context.Wallets.Verify(x => x.AddAsync(
                It.Is<UserWallet>(wallet =>
                    wallet.UserProfileId == context.Profile.UserProfileId &&
                    wallet.CreditBalance == 1_000 &&
                    wallet.CreatedBy == context.Profile.IdentityUserId),
                It.IsAny<CancellationToken>()),
            Times.Once);
        VerifySuccessfulTopUp(context, expectedBalance: 1_000);
    }

    [Fact]
    public async Task TopUpAsync_WhenPackageDoesNotExist_ThrowsWithoutFinancialWrites()
    {
        var context = CreateContext();
        context.Packages.Setup(x => x.GetByIdAsync(
                404,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((CreditPackage?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            context.Service.TopUpAsync(
                context.Profile.IdentityUserId,
                new TopUpRequest { CreditPackageId = 404 }));

        context.Wallets.Verify(
            x => x.AddAsync(It.IsAny<UserWallet>(), It.IsAny<CancellationToken>()),
            Times.Never);
        context.Payments.Verify(
            x => x.AddAsync(It.IsAny<UserPayment>(), It.IsAny<CancellationToken>()),
            Times.Never);
        context.Transactions.Verify(
            x => x.AddAsync(It.IsAny<FinancialTransaction>(), It.IsAny<CancellationToken>()),
            Times.Never);
        context.UnitOfWork.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task TopUpAsync_WhenProfileDoesNotExist_ThrowsBeforePackageQuery()
    {
        var context = CreateContext(profileExists: false);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            context.Service.TopUpAsync(
                "missing-user",
                new TopUpRequest { CreditPackageId = 1 }));

        context.Packages.Verify(
            x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
        context.UnitOfWork.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private static void SetupSuccessfulTopUp(
        WalletServiceContext context,
        UserWallet? wallet)
    {
        context.Packages.Setup(x => x.GetByIdAsync(
                context.Package.CreditPackageId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(context.Package);
        context.Wallets.Setup(x => x.GetByUserProfileIdAsync(
                context.Profile.UserProfileId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(wallet);
        context.Wallets.Setup(x => x.AddAsync(
                It.IsAny<UserWallet>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        context.Payments.Setup(x => x.AddAsync(
                It.IsAny<UserPayment>(),
                It.IsAny<CancellationToken>()))
            .Callback<UserPayment, CancellationToken>((payment, _) =>
                payment.UserPaymentId = 55)
            .Returns(Task.CompletedTask);
        context.Transactions.Setup(x => x.AddAsync(
                It.IsAny<FinancialTransaction>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        context.UnitOfWork.Setup(x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(3);
    }

    private static void VerifySuccessfulTopUp(
        WalletServiceContext context,
        long expectedBalance)
    {
        context.Payments.Verify(x => x.AddAsync(
                It.Is<UserPayment>(payment =>
                    payment.UserProfileId == context.Profile.UserProfileId &&
                    payment.CurrencyCode == context.Package.CurrencyCode &&
                    payment.Amount == context.Package.Amount &&
                    payment.CreditsPurchased == context.Package.Credits &&
                    payment.PaymentProvider == "IntertwineDemo" &&
                    payment.Status == PaymentStatus.Completed),
                It.IsAny<CancellationToken>()),
            Times.Once);
        context.Transactions.Verify(x => x.AddAsync(
                It.Is<FinancialTransaction>(transaction =>
                    transaction.CreditAmount == context.Package.Credits &&
                    transaction.BalanceAfterTransaction == expectedBalance &&
                    transaction.TransactionType ==
                        FinancialTransactionType.CreditPurchase &&
                    transaction.UserPayment != null &&
                    transaction.UserWallet != null),
                It.IsAny<CancellationToken>()),
            Times.Once);
        context.UnitOfWork.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private static WalletServiceContext CreateContext(bool profileExists = true)
    {
        var profile = new UserProfile
        {
            UserProfileId = 12,
            IdentityUserId = "identity-12"
        };
        var package = new CreditPackage
        {
            CreditPackageId = 4,
            CurrencyCode = "AUD",
            Amount = 9.99m,
            Credits = 1_000
        };
        var profiles = new Mock<IUserProfileRepository>();
        profiles.Setup(x => x.GetByIdentityUserIdAsync(It.IsAny<string>()))
            .ReturnsAsync(profileExists ? profile : null);
        var wallets = new Mock<IUserWalletRepository>();
        var packages = new Mock<ICreditPackageRepository>();
        var payments = new Mock<IUserPaymentRepository>();
        var transactions = new Mock<IFinancialTransactionRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        return new WalletServiceContext(
            profile,
            package,
            profiles,
            wallets,
            packages,
            payments,
            transactions,
            unitOfWork);
    }

    private sealed class WalletServiceContext
    {
        public WalletServiceContext(
            UserProfile profile,
            CreditPackage package,
            Mock<IUserProfileRepository> profiles,
            Mock<IUserWalletRepository> wallets,
            Mock<ICreditPackageRepository> packages,
            Mock<IUserPaymentRepository> payments,
            Mock<IFinancialTransactionRepository> transactions,
            Mock<IUnitOfWork> unitOfWork)
        {
            Profile = profile;
            Package = package;
            Profiles = profiles;
            Wallets = wallets;
            Packages = packages;
            Payments = payments;
            Transactions = transactions;
            UnitOfWork = unitOfWork;
            Service = new Service(
                profiles.Object,
                wallets.Object,
                packages.Object,
                payments.Object,
                transactions.Object,
                unitOfWork.Object);
        }

        public UserProfile Profile { get; }
        public CreditPackage Package { get; }
        public Mock<IUserProfileRepository> Profiles { get; }
        public Mock<IUserWalletRepository> Wallets { get; }
        public Mock<ICreditPackageRepository> Packages { get; }
        public Mock<IUserPaymentRepository> Payments { get; }
        public Mock<IFinancialTransactionRepository> Transactions { get; }
        public Mock<IUnitOfWork> UnitOfWork { get; }
        public Service Service { get; }
    }
}
