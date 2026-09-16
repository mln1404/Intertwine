using Intertwine.Services.DTOs.Currencies;
using Intertwine.Services.Interfaces;
using Intertwine.Services.Interfaces.Repositories;

namespace Intertwine.Services.Services;

public class CurrencyService : ICurrencyService
{
    private readonly ICurrencyRepository _currencyRepository;

    public CurrencyService(ICurrencyRepository currencyRepository)
    {
        _currencyRepository = currencyRepository;
    }

    public async Task<IReadOnlyList<CurrencyDto>> GetActiveAsync(
        CancellationToken cancellationToken = default)
    {
        var currencies = await _currencyRepository.GetActiveAsync(cancellationToken);

        return currencies.Select(x => new CurrencyDto
        {
            Code = x.Code,
            Symbol = x.Symbol,
            Name = x.Name,
            DecimalPlaces = x.DecimalPlaces
        }).ToList();
    }
}
