namespace Intertwine.Services.Interfaces.Repositories;

/// <summary>
/// Provides persistence access to supported currencies.
/// </summary>
public interface ICurrencyRepository
{
    /// <summary>Gets active currencies.</summary>
    Task<IReadOnlyList<Currency>> GetActiveAsync(
        CancellationToken cancellationToken = default);
}
