using Intertwine.Services.DTOs.Currencies;

namespace Intertwine.Services.Interfaces;

/// <summary>
/// Retrieves currencies supported by credit packages.
/// </summary>
public interface ICurrencyService
{
    /// <summary>Gets active currencies.</summary>
    Task<IReadOnlyList<CurrencyDto>> GetActiveAsync(
        CancellationToken cancellationToken = default);
}
