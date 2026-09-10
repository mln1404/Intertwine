namespace Intertwine.Domain.Interfaces;

/// <summary>
/// Marker interface for domain entities that exposes common audit properties.
/// </summary>
public interface IEntity
{
    /// <summary>
    /// UTC date and time when the entity was created.
    /// </summary>
    DateTime DateCreated { get; set; }

    /// <summary>
    /// UTC date and time when the entity was last updated, if any.
    /// </summary>
    DateTime? DateUpdated { get; set; }

    /// <summary>
    /// Identifier of the user who created the entity.
    /// </summary>
    string? CreatedBy { get; set; }

    /// <summary>
    /// Identifier of the user who last updated the entity.
    /// </summary>
    string? UpdatedBy { get; set; }
}
