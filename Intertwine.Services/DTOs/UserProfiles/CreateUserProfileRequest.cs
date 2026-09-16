using System.ComponentModel.DataAnnotations;

namespace Intertwine.Services.DTOs.UserProfiles;

public class CreateUserProfileRequest
{
    [Required]
    [MaxLength(200)]
    public string AvatarName { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string LastName { get; set; } = string.Empty;

    [MaxLength(200)]
    public string MiddleName { get; set; } = string.Empty;
}
