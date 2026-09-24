using Intertwine.Domain.Abstractions;

namespace Intertwine.Domain.Entities;

/// <summary>
/// Represents a supported personality classification that may be assigned to user profiles.
/// </summary>
public class PersonalityType : ActivatableEntity
{
    /// <summary>
    /// Gets or sets the personality type identifier.
    /// </summary>
    public int PersonalityTypeId { get; set; }

    /// <summary>
    /// Gets or sets the unique four-letter personality type code.
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets an optional display name for the personality type.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets an optional description of the personality type.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the profiles assigned to this personality type.
    /// </summary>
    public ICollection<UserProfile> UserProfiles { get; set; } = [];
}
