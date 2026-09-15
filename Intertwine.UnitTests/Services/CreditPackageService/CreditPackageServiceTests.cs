using Intertwine.Services.Interfaces.Repositories;
using Moq;
using Service = Intertwine.Services.Services.CreditPackageService;

namespace Intertwine.UnitTests.Services.CreditPackageService;

public class CreditPackageServiceTests
{
    [Fact]
    public async Task GetActiveAsync_WhenCurrencyIsBlank_ThrowsWithoutQueryingRepository()
    {
        var repository = new Mock<ICreditPackageRepository>(MockBehavior.Strict);
        var service = new Service(repository.Object);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.GetActiveAsync("  "));

        repository.Verify(
            x => x.GetActiveByCurrencyAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetActiveAsync_NormalizesCurrencyAndMapsPackages()
    {
        var repository = new Mock<ICreditPackageRepository>(MockBehavior.Strict);
        repository.Setup(x => x.GetActiveByCurrencyAsync(
                "AUD",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([
                new CreditPackage
                {
                    CreditPackageId = 4,
                    CurrencyCode = "AUD",
                    Amount = 9.99m,
                    Credits = 1_000
                }
            ]);
        var service = new Service(repository.Object);

        var result = (await service.GetActiveAsync(" aud ")).Single();

        Assert.Equal(4, result.CreditPackageId);
        Assert.Equal("AUD", result.CurrencyCode);
        Assert.Equal(9.99m, result.Amount);
        Assert.Equal(1_000, result.Credits);
        repository.VerifyAll();
    }
}
