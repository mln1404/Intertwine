using Intertwine.Domain.Entities;

namespace Intertwine.Services.Interfaces.Repositories;

/// <summary>
/// Provides persistence access to supported personality types.
/// </summary>
public interface IPersonalityTypeRepository
{
    /// <summary>Gets active personality types ordered by code.</summary>
    Task<IReadOnlyList<PersonalityType>> GetActiveAsync(
        CancellationToken cancellationToken = default);

    /// <summary>Gets an active personality type by its identifier.</summary>
    Task<PersonalityType?> GetActiveByIdAsync(
        int personalityTypeId,
        CancellationToken cancellationToken = default);
}
