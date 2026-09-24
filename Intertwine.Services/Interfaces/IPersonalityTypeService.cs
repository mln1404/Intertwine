using Intertwine.Services.DTOs.PersonalityTypes;

namespace Intertwine.Services.Interfaces;

/// <summary>
/// Retrieves personality types available for user profiles.
/// </summary>
public interface IPersonalityTypeService
{
    /// <summary>Gets active personality types ordered by code.</summary>
    Task<IReadOnlyList<PersonalityTypeDto>> GetActiveAsync(
        CancellationToken cancellationToken = default);
}
