namespace Intertwine.Services.DTOs.UserProfiles;

public class UpdateUserProfileRequest
{
    public string AvatarName { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string MiddleName { get; set; } = string.Empty;

    public int? PersonalityTypeId { get; set; }
}
