namespace Intertwine.Services.Constants;

/// <summary>
/// User-facing messages returned while submitting an answer.
/// </summary>
public static class UserAnswerMessages
{
    public const string UserProfileNotFound = "User profile not found.";
    public const string AnswerDoesNotBelongToQuestion =
        "The selected answer does not belong to this question.";
    public const string ExistingNonDailyAnswer =
        "You have already selected this answer for this question.";
    public const string NonDailyQuestionLimitReached =
        "Your two free question actions are used. Confirm spending 10 Sparks for an extra answer or update.";
    public const string DailyQuestionAlreadyAnswered =
        "You have already answered today's Daily Question.";
}
