namespace Intertwine.Services.DTOs.UserAnswers;

/// <summary>
/// Identifies a user's current answer for one question.
/// </summary>
public class UserAnswerSelectionDto
{
    public int QuestionId { get; set; }

    public int AnswerId { get; set; }
}
