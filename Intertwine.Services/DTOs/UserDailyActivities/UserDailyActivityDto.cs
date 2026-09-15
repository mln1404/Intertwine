namespace Intertwine.Services.DTOs.UserDailyActivities;

/// <summary>
/// Describes the user's answer allowances for one local calendar date.
/// </summary>
public class UserDailyActivityDto
{
    public DateOnly LocalDate { get; set; }

    public bool DailyQuestionCreateOrUpdateUsed { get; set; }

    public int NonDailyQuestionsAnswered { get; set; }

    public int NonDailyQuestionsRemaining { get; set; }
}
