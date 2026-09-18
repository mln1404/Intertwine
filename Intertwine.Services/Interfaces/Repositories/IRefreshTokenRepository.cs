using Intertwine.Identity;

namespace Intertwine.Services.Interfaces.Repositories;

/// <summary>
/// Persists hashed refresh-token records.
/// </summary>
public interface IRefreshTokenRepository
{
    /// <summary>Adds a hashed refresh-token record to the current unit of work.</summary>
    Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);

    /// <summary>Gets a refresh token by its persisted hash, including its user.</summary>
    Task<RefreshToken?> GetByHashAsync(string tokenHash, CancellationToken cancellationToken = default);
}
