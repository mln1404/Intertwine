using Intertwine.Domain.Entities;
using Intertwine.Repositories.Data;
using Intertwine.Services.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Intertwine.Repositories.Repositories;

/// <summary>
/// EF Core repository for credit packages.
/// </summary>
public class CreditPackageRepository
    : ICreditPackageRepository
{
    private readonly IntertwineDbContext _context;

    public CreditPackageRepository(
        IntertwineDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<CreditPackage>>
    GetActiveByCurrencyAsync(
        string currencyCode,
        CancellationToken cancellationToken = default)
    {
        return await _context.CreditPackages
            .AsNoTracking()
            .Where(x =>
                x.CurrencyCode == currencyCode &&
                x.IsActive &&
                x.Currency.IsActive)
            .OrderBy(x => x.Amount)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<CreditPackage?> GetByIdAsync(
        int creditPackageId,
        CancellationToken cancellationToken = default)
    {
        return await _context.CreditPackages
            .AsNoTracking()
            .Include(x => x.Currency)
            .FirstOrDefaultAsync(
                x =>
                    x.CreditPackageId == creditPackageId &&
                    x.IsActive &&
                    x.Currency.IsActive,
                cancellationToken);
    }
}
