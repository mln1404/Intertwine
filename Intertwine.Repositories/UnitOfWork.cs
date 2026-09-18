using Intertwine.Repositories.Data;
using Intertwine.Services.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Intertwine.Repositories;

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
    /// <inheritdoc />
    public async Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new InvalidOperationException("Your balance or activity changed in another request. Refresh and try again.", ex);
        }
    }
}
