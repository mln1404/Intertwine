namespace Intertwine.Services.Interfaces.Repositories;

public interface IFinancialTransactionRepository
{
    Task AddAsync(
        FinancialTransaction transaction,
        CancellationToken cancellationToken = default);
}
