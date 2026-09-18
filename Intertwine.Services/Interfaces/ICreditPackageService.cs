using Intertwine.Services.DTOs.CreditPackages;

namespace Intertwine.Services.Interfaces;

/// <summary>
/// Retrieves purchasable Spark packages.
/// </summary>
public interface ICreditPackageService
{
    /// <summary>Gets active packages that are priced in the requested currency.</summary>
    Task<IEnumerable<CreditPackageDto>> GetActiveAsync(
        string currencyCode,
        CancellationToken cancellationToken = default);
}
