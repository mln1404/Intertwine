using Intertwine.Domain.Entities;
using Intertwine.Repositories.Data;
using Intertwine.Services.Interfaces.Repositories;

namespace Intertwine.Repositories.Repositories;

public class FinancialTransactionRepository
    : IFinancialTransactionRepository
{
    private readonly IntertwineDbContext _context;

    public FinancialTransactionRepository(
        IntertwineDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(
        FinancialTransaction transaction,
        CancellationToken cancellationToken = default)
    {
        await _context.FinancialTransactions.AddAsync(
            transaction,
            cancellationToken);
    }
}
