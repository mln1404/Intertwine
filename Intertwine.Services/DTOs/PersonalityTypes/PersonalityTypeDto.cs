namespace Intertwine.Services.DTOs.PersonalityTypes;

/// <summary>
/// Describes an active personality type available for profile selection.
/// </summary>
public class PersonalityTypeDto
{
    public int PersonalityTypeId { get; set; }

    public string Code { get; set; } = string.Empty;

    public string? Name { get; set; }
}
