using Intertwine.Domain.Entities;
using Intertwine.Repositories.Data;
using Intertwine.Services.Interfaces.Repositories;

namespace Intertwine.Repositories.Repositories;

/// <summary>
/// EF Core repository that stages financial ledger entries.
/// </summary>
public class FinancialTransactionRepository
    : IFinancialTransactionRepository
{
    private readonly IntertwineDbContext _context;

    public FinancialTransactionRepository(
        IntertwineDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task AddAsync(
        FinancialTransaction transaction,
        CancellationToken cancellationToken = default)
    {
        await _context.FinancialTransactions.AddAsync(
            transaction,
            cancellationToken);
    }
}
