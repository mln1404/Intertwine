using Intertwine.Domain.Entities;
using Intertwine.Repositories.Data;
using Intertwine.Services.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Intertwine.Repositories.Repositories;

/// <summary>
/// EF Core repository for active personality types.
/// </summary>
public class PersonalityTypeRepository : IPersonalityTypeRepository
{
    private readonly IntertwineDbContext _context;

    public PersonalityTypeRepository(IntertwineDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PersonalityType>> GetActiveAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.PersonalityTypes
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Code)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<PersonalityType?> GetActiveByIdAsync(
        int personalityTypeId,
        CancellationToken cancellationToken = default)
    {
        return await _context.PersonalityTypes
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.PersonalityTypeId == personalityTypeId && x.IsActive,
                cancellationToken);
    }
}
