using Intertwine.Services.DTOs.Discovery;

namespace Intertwine.Services.Interfaces;

public interface IDailyQuestionDiscoveryService
{
    Task<DailyQuestionDiscoveryDto?> GetContextAsync(
        string identityUserId,
        int dailyQuestionId,
        DateOnly localDate,
        CancellationToken cancellationToken = default);

    Task<DiscoveryUsersDto?> GetUsersAsync(
        string identityUserId,
        int dailyQuestionId,
        int answerId,
        DateOnly localDate,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}
