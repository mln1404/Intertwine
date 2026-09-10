using Intertwine.Domain.Interfaces;

namespace Intertwine.Domain.Entities;

/// <summary>
/// Base properties common to all entities in the domain.
/// </summary>
public abstract class BaseEntity : IEntity
{
    /// <summary>
    /// UTC date and time when the entity was created.
    /// </summary>
    public DateTime DateCreated { get; set; }

    /// <summary>
    /// UTC date and time when the entity was last updated, if any.
    /// </summary>
    public DateTime? DateUpdated { get; set; }

    /// <summary>
    /// Identifier for the user who created the entity.
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Identifier for the user who last updated the entity.
    /// </summary>
    public string? UpdatedBy { get; set; }
}
