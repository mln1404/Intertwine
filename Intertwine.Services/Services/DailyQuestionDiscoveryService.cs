using Intertwine.Services.Constants;
using Intertwine.Services.DTOs.Discovery;
using Intertwine.Services.Interfaces;
using Intertwine.Services.Interfaces.Repositories;

namespace Intertwine.Services.Services;

/// <summary>
/// Builds Daily Question discovery context and enforces its access policy.
/// </summary>
public sealed class DailyQuestionDiscoveryService(
    IDailyQuestionDiscoveryRepository repository,
    IDailyQuestionDiscoveryAccessPolicy accessPolicy)
    : IDailyQuestionDiscoveryService
{
    public async Task<DailyQuestionDiscoveryDto?> GetContextAsync(
        string identityUserId,
        int dailyQuestionId,
        DateOnly localDate,
        CancellationToken cancellationToken = default)
    {
        var context = await GetAnsweredContextAsync(
            identityUserId,
            dailyQuestionId,
            cancellationToken);
        if (context is null)
            return null;

        var dateAccess = accessPolicy.GetDateAccess(context.Date, localDate);
        var history = await repository.GetHistoryAsync(
            localDate.AddDays(-DailyQuestionDiscoveryRules.SupportedHistoryDays),
            localDate,
            identityUserId,
            cancellationToken);

        return new DailyQuestionDiscoveryDto
        {
            DailyQuestionId = context.DailyQuestionId,
            Date = context.Date,
            QuestionId = context.QuestionId,
            QuestionTitle = context.QuestionTitle,
            FullQuestion = context.FullQuestion,
            CurrentUserAnswerId = context.CurrentUserAnswerId!.Value,
            CurrentUserAnswerText = context.CurrentUserAnswerText ?? string.Empty,
            DateAccessState = dateAccess,
            AnswerPools = context.Answers.Select(answer => new DiscoveryAnswerPoolDto
            {
                AnswerId = answer.AnswerId,
                AnswerText = answer.AnswerText,
                AccessState = accessPolicy.GetAnswerPoolAccess(
                    dateAccess,
                    context.CurrentUserAnswerId.Value,
                    answer.AnswerId)
            }).ToList(),
            HistoricalAccess = history.Select(day => new DiscoveryDayDto
            {
                DailyQuestionId = day.DailyQuestionId,
                Date = day.Date,
                QuestionTitle = day.QuestionTitle,
                HasAnswered = day.HasAnswered,
                AccessState = accessPolicy.GetDateAccess(day.Date, localDate)
            }).ToList()
        };
    }

    public async Task<DiscoveryUsersDto?> GetUsersAsync(
        string identityUserId,
        int dailyQuestionId,
        int answerId,
        DateOnly localDate,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        if (page < 1)
            throw new ArgumentOutOfRangeException(nameof(page));
        if (pageSize < 1 || pageSize > DailyQuestionDiscoveryRules.MaximumPageSize)
            throw new ArgumentOutOfRangeException(nameof(pageSize));

        var context = await GetAnsweredContextAsync(
            identityUserId,
            dailyQuestionId,
            cancellationToken);
        if (context is null)
            return null;

        if (context.Answers.All(x => x.AnswerId != answerId))
            throw new ArgumentException(DailyQuestionDiscoveryMessages.AnswerDoesNotBelong);

        var dateAccess = accessPolicy.GetDateAccess(context.Date, localDate);
        var poolAccess = accessPolicy.GetAnswerPoolAccess(
            dateAccess,
            context.CurrentUserAnswerId!.Value,
            answerId);

        if (poolAccess != DiscoveryAccessState.Included)
        {
            return new DiscoveryUsersDto
            {
                AccessState = poolAccess,
                Page = page,
                PageSize = pageSize
            };
        }

        var result = await repository.GetUsersAsync(
            answerId,
            identityUserId,
            page,
            pageSize,
            cancellationToken);

        return new DiscoveryUsersDto
        {
            AccessState = poolAccess,
            Page = page,
            PageSize = pageSize,
            TotalCount = result.TotalCount,
            HasMore = page * pageSize < result.TotalCount,
            Users = result.Users
        };
    }

    private async Task<DiscoveryContextData?> GetAnsweredContextAsync(
        string identityUserId,
        int dailyQuestionId,
        CancellationToken cancellationToken)
    {
        var context = await repository.GetContextAsync(
            dailyQuestionId,
            identityUserId,
            cancellationToken);

        if (context is not null && context.CurrentUserAnswerId is null)
            throw new InvalidOperationException(DailyQuestionDiscoveryMessages.AnswerRequired);

        return context;
    }
}
