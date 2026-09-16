using Intertwine.Services.Interfaces.Repositories;
using Intertwine.Services.Services;
using Moq;

namespace Intertwine.UnitTests.Services.CurrencyService;

public class CurrencyServiceTests
{
    [Fact]
    public async Task GetActiveAsync_MapsRepositoryCurrencies()
    {
        var repository = new Mock<ICurrencyRepository>();
        repository.Setup(x => x.GetActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([
                new Currency
                {
                    Code = "AUD",
                    Symbol = "$",
                    Name = "Australian Dollar",
                    DecimalPlaces = 2
                }
            ]);
        var service = new Service(repository.Object);

        var result = await service.GetActiveAsync();

        var currency = Assert.Single(result);
        Assert.Equal("AUD", currency.Code);
        Assert.Equal("Australian Dollar", currency.Name);
        Assert.Equal("$", currency.Symbol);
        Assert.Equal(2, currency.DecimalPlaces);
    }

    private sealed class Service : Intertwine.Services.Services.CurrencyService
    {
        public Service(ICurrencyRepository repository) : base(repository)
        {
        }
    }
}
