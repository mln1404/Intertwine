using Intertwine.Repositories.Data;
using Intertwine.Services.Interfaces.Repositories;

namespace Intertwine.Repositories.Repositories;

/// <summary>
/// EF Core-backed unit of work for the current database context.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly IntertwineDbContext _context;

    public UnitOfWork(IntertwineDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
