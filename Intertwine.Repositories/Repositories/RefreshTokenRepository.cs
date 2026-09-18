using Intertwine.Identity;
using Intertwine.Repositories.Data;
using Intertwine.Services.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// EF Core repository for hashed refresh-token records.
/// </summary>
public class RefreshTokenRepository
    : IRefreshTokenRepository
{
    private readonly IntertwineDbContext _context;

    public RefreshTokenRepository(
        IntertwineDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task AddAsync(
        RefreshToken refreshToken,
        CancellationToken cancellationToken = default)
    {
        await _context.RefreshTokens.AddAsync(
            refreshToken,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<RefreshToken?> GetByHashAsync(
        string tokenHash,
        CancellationToken cancellationToken = default)
    {
        return _context.RefreshTokens
            .Include(x => x.User)
            .FirstOrDefaultAsync(
                x => x.TokenHash == tokenHash,
                cancellationToken);
    }
}
