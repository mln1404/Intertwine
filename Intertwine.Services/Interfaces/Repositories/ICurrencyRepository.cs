namespace Intertwine.Services.Interfaces.Repositories;

public interface ICurrencyRepository
{
    Task<IReadOnlyList<Currency>> GetActiveAsync(
        CancellationToken cancellationToken = default);
}
