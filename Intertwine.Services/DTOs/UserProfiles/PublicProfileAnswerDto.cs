using Intertwine.Services.DTOs.Categories;

namespace Intertwine.Services.DTOs.UserProfiles;

/// <summary>
/// Describes one current answer that is safe to display on a public profile.
/// </summary>
public class PublicProfileAnswerDto
{
    public int QuestionId { get; set; }

    public string QuestionTitle { get; set; } = string.Empty;

    public string FullQuestion { get; set; } = string.Empty;

    public int AnswerId { get; set; }

    public string AnswerText { get; set; } = string.Empty;

    public IReadOnlyList<CategoryDto> Categories { get; set; } = [];
}
