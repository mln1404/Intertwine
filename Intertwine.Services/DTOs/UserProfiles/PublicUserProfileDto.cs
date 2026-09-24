namespace Intertwine.Services.DTOs.UserProfiles;

/// <summary>
/// Contains only profile information intended to be visible to another user.
/// </summary>
public class PublicUserProfileDto
{
    public int UserProfileId { get; set; }

    public string AvatarName { get; set; } = string.Empty;

    public string? PersonalityTypeCode { get; set; }

    public IReadOnlyList<PublicProfileAnswerDto> AnsweredQuestions { get; set; } = [];
}
