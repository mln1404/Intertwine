namespace Intertwine.Services.Interfaces.Repositories;

/// <summary>
/// Commits changes staged by repositories within the current request scope.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Persists all tracked changes as one EF Core save operation.
    /// </summary>
    Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default);
}
