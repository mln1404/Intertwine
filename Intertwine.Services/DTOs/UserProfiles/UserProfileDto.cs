namespace Intertwine.Services.DTOs.UserProfiles;

public class UserProfileDto
{
    public int UserProfileId { get; set; }

    public string AvatarName { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string MiddleName { get; set; } = string.Empty;

    public string IdentityUserId { get; set; } = string.Empty;

    public long CreditBalance { get; set; }
}
