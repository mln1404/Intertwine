namespace Intertwine.Services.Interfaces.Repositories
{
    /// <summary>
    /// Provides persistence access to Spark packages.
    /// </summary>
    public interface ICreditPackageRepository
    {
        /// <summary>Gets active packages that use the supplied currency.</summary>
        Task<IEnumerable<CreditPackage>> GetActiveByCurrencyAsync(
            string currencyCode,
            CancellationToken cancellationToken = default);

        /// <summary>Gets one package by identifier.</summary>
        Task<CreditPackage?> GetByIdAsync(
            int creditPackageId,
            CancellationToken cancellationToken = default);
    }
}
