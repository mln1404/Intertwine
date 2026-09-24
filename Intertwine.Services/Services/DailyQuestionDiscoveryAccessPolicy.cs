using Intertwine.Services.DTOs.Discovery;
using Intertwine.Services.Interfaces;
using Intertwine.Services.Constants;

namespace Intertwine.Services.Services;

public sealed class DailyQuestionDiscoveryAccessPolicy
    : IDailyQuestionDiscoveryAccessPolicy
{
    public DiscoveryAccessState GetDateAccess(
        DateOnly dailyQuestionDate,
        DateOnly localDate)
    {
        var daysAgo = localDate.DayNumber - dailyQuestionDate.DayNumber;

        return daysAgo switch
        {
            >= 0 and <= DailyQuestionDiscoveryRules.FreeHistoryDays =>
                DiscoveryAccessState.Included,
            > DailyQuestionDiscoveryRules.FreeHistoryDays and
                <= DailyQuestionDiscoveryRules.SupportedHistoryDays =>
                DiscoveryAccessState.SubscriptionRequired,
            _ => DiscoveryAccessState.Unavailable
        };
    }

    public DiscoveryAccessState GetAnswerPoolAccess(
        DiscoveryAccessState dateAccess,
        int currentAnswerId,
        int requestedAnswerId)
    {
        if (dateAccess == DiscoveryAccessState.Unavailable)
            return DiscoveryAccessState.Unavailable;

        return currentAnswerId == requestedAnswerId
            ? dateAccess
            : DiscoveryAccessState.SparksRequired;
    }
}
