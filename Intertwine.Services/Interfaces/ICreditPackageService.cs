using Intertwine.Services.DTOs.CreditPackages;

namespace Intertwine.Services.Interfaces;

public interface ICreditPackageService
{
    Task<IEnumerable<CreditPackageDto>> GetActiveAsync(
        string currencyCode,
        CancellationToken cancellationToken = default);
}
