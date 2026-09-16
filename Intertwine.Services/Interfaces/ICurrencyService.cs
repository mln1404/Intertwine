using Intertwine.Services.DTOs.Currencies;

namespace Intertwine.Services.Interfaces;

public interface ICurrencyService
{
    Task<IReadOnlyList<CurrencyDto>> GetActiveAsync(
        CancellationToken cancellationToken = default);
}
