using Intertwine.Domain.Abstractions;

namespace Intertwine.Domain.Entities;

/// <summary>
/// Records a user's answer activity for one client-local calendar date.
/// </summary>
public class UserDailyActivity : BaseEntity
{
    /// <summary>
    /// Primary key for the activity record.
    /// </summary>
    public int UserDailyActivityId { get; set; }

    /// <summary>
    /// Foreign key for the user whose activity is recorded.
    /// </summary>
    public int UserProfileId { get; set; }

    /// <summary>
    /// User profile that owns this activity record.
    /// </summary>
    public UserProfile UserProfile { get; set; } = null!;

    /// <summary>
    /// Client-local date to which the activity applies.
    /// </summary>
    public DateOnly Date { get; set; }

    /// <summary>
    /// Number of newly answered non-Daily Questions for this date.
    /// </summary>
    public int NonDailyQuestionsAnswered { get; set; }

    /// <summary>
    /// Whether the user has already submitted the Daily Question for this date.
    /// </summary>
    public bool DailyQuestionCreateOrUpdateUsed { get; set; }
}
