using Intertwine.Services.DTOs.Discovery;

namespace Intertwine.Services.Interfaces;

public interface IDailyQuestionDiscoveryAccessPolicy
{
    DiscoveryAccessState GetDateAccess(DateOnly dailyQuestionDate, DateOnly localDate);

    DiscoveryAccessState GetAnswerPoolAccess(
        DiscoveryAccessState dateAccess,
        int currentAnswerId,
        int requestedAnswerId);
}
