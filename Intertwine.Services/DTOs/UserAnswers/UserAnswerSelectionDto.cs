namespace Intertwine.Services.DTOs.UserAnswers;

/// <summary>
/// Identifies a user's current answer for one question.
/// </summary>
public class UserAnswerSelectionDto
{
    public int QuestionId { get; set; }

    public int AnswerId { get; set; }
    public string AnswerText { get; set; } = string.Empty;
    public string QuestionTitle { get; set; } = string.Empty;
    public string FullQuestion { get; set; } = string.Empty;
    public IReadOnlyList<Intertwine.Services.DTOs.Categories.CategoryDto> Categories { get; set; } = [];
}
