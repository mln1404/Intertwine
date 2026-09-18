using Intertwine.Services.DTOs.CreditPackages;
using Intertwine.Services.Interfaces;
using Intertwine.Services.Interfaces.Repositories;

namespace Intertwine.Services.Services;

/// <summary>
/// Maps active credit packages to API response data.
/// </summary>
public class CreditPackageService : ICreditPackageService
{
    private readonly ICreditPackageRepository
        _creditPackageRepository;

    public CreditPackageService(
        ICreditPackageRepository creditPackageRepository)
    {
        _creditPackageRepository = creditPackageRepository;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<CreditPackageDto>> GetActiveAsync(
        string currencyCode,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(currencyCode))
        {
            throw new ArgumentException(
                "Currency code is required.");
        }

        currencyCode = currencyCode
            .Trim()
            .ToUpperInvariant();

        var packages =
            await _creditPackageRepository
                .GetActiveByCurrencyAsync(
                    currencyCode,
                    cancellationToken);

        return packages.Select(x => new CreditPackageDto
        {
            CreditPackageId = x.CreditPackageId,
            CurrencyCode = x.CurrencyCode,
            Amount = x.Amount,
            Credits = x.Credits
        });
    }
}
