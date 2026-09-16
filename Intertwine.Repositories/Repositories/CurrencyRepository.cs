using Intertwine.Repositories.Data;
using Intertwine.Services.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Intertwine.Repositories.Repositories;

public class CurrencyRepository : ICurrencyRepository
{
    private readonly IntertwineDbContext _context;

    public CurrencyRepository(IntertwineDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Currency>> GetActiveAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.Currencies
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }
}
