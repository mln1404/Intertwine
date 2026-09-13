using Intertwine.Domain.Abstractions;

namespace Intertwine.Domain.Entities;

public class UserDailyActivity : BaseEntity
{
    public int UserDailyActivityId { get; set; }

    public int UserProfileId { get; set; }
    public UserProfile UserProfile { get; set; } = null!;

    public DateOnly Date { get; set; }

    public int NonDailyQuestionsAnswered { get; set; }

    public bool DailyQuestionCreateOrUpdateUsed { get; set; }
}
