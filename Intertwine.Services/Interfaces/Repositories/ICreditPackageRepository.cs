namespace Intertwine.Services.Interfaces.Repositories
{
    public interface ICreditPackageRepository
    {
        Task<IEnumerable<CreditPackage>> GetActiveByCurrencyAsync(
            string currencyCode,
            CancellationToken cancellationToken = default);

        Task<CreditPackage?> GetByIdAsync(
            int creditPackageId,
            CancellationToken cancellationToken = default);
    }
}
