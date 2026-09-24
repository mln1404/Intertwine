using Intertwine.Services.DTOs.Discovery;

namespace Intertwine.Services.Interfaces.Repositories;

public interface IDailyQuestionDiscoveryRepository
{
    Task<DiscoveryContextData?> GetContextAsync(
        int dailyQuestionId,
        string identityUserId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DiscoveryHistoryData>> GetHistoryAsync(
        DateOnly startDate,
        DateOnly endDate,
        string identityUserId,
        CancellationToken cancellationToken = default);

    Task<DiscoveryUsersPageData> GetUsersAsync(
        int answerId,
        string requestingIdentityUserId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}
