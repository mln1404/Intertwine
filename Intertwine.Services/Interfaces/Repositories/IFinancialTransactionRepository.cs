namespace Intertwine.Services.Interfaces.Repositories;

/// <summary>
/// Stages financial ledger entries for the current unit of work.
/// </summary>
public interface IFinancialTransactionRepository
{
    /// <summary>Adds a ledger entry without committing it independently.</summary>
    Task AddAsync(
        FinancialTransaction transaction,
        CancellationToken cancellationToken = default);
}
