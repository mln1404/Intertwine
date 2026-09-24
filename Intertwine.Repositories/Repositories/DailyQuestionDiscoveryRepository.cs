using Intertwine.Repositories.Data;
using Intertwine.Services.DTOs.Discovery;
using Intertwine.Services.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Intertwine.Repositories.Repositories;

/// <summary>
/// Executes read-only Daily Question discovery queries against current answers.
/// </summary>
public sealed class DailyQuestionDiscoveryRepository(IntertwineDbContext context)
    : IDailyQuestionDiscoveryRepository
{
    public async Task<DiscoveryContextData?> GetContextAsync(
        int dailyQuestionId,
        string identityUserId,
        CancellationToken cancellationToken = default)
    {
        var dailyQuestion = await context.DailyQuestions
            .AsNoTracking()
            .Where(x => x.DailyQuestionId == dailyQuestionId)
            .Select(x => new
            {
                x.DailyQuestionId,
                x.Date,
                x.QuestionId,
                x.Question.QuestionTitle,
                x.Question.FullQuestion,
                Answers = x.Question.Answers
                    .Where(a => a.IsActive)
                    .OrderBy(a => a.AnswerId)
                    .Select(a => new DiscoveryAnswerData(a.AnswerId, a.AnswerText))
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (dailyQuestion is null)
            return null;

        var currentAnswer = await context.UserAnswers
            .AsNoTracking()
            .Where(x =>
                x.UserProfile.IsActive &&
                x.UserProfile.IdentityUserId == identityUserId &&
                x.Answer.QuestionId == dailyQuestion.QuestionId)
            .Select(x => new { x.AnswerId, x.Answer.AnswerText })
            .FirstOrDefaultAsync(cancellationToken);

        return new DiscoveryContextData(
            dailyQuestion.DailyQuestionId,
            dailyQuestion.Date,
            dailyQuestion.QuestionId,
            dailyQuestion.QuestionTitle,
            dailyQuestion.FullQuestion,
            currentAnswer?.AnswerId,
            currentAnswer?.AnswerText,
            dailyQuestion.Answers);
    }

    public async Task<IReadOnlyList<DiscoveryHistoryData>> GetHistoryAsync(
        DateOnly startDate,
        DateOnly endDate,
        string identityUserId,
        CancellationToken cancellationToken = default)
    {
        return await context.DailyQuestions
            .AsNoTracking()
            .Where(x => x.Date >= startDate && x.Date <= endDate)
            .OrderByDescending(x => x.Date)
            .Select(x => new DiscoveryHistoryData(
                x.DailyQuestionId,
                x.Date,
                x.Question.QuestionTitle,
                context.UserAnswers.Any(ua =>
                    ua.UserProfile.IsActive &&
                    ua.UserProfile.IdentityUserId == identityUserId &&
                    ua.Answer.QuestionId == x.QuestionId)))
            .ToListAsync(cancellationToken);
    }

    public async Task<DiscoveryUsersPageData> GetUsersAsync(
        int answerId,
        string requestingIdentityUserId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var matchingUsers = context.UserAnswers
            .AsNoTracking()
            .Where(x =>
                x.AnswerId == answerId &&
                x.UserProfile.IsActive &&
                x.UserProfile.IdentityUserId != requestingIdentityUserId);

        var totalCount = await matchingUsers.CountAsync(cancellationToken);
        var users = await matchingUsers
            .OrderBy(x => x.UserProfileId)
            .Select(x => new DiscoveryUserDto
            {
                UserProfileId = x.UserProfileId,
                AvatarName = x.UserProfile.AvatarName,
                PersonalityTypeCode = x.UserProfile.PersonalityType == null
                    ? null
                    : x.UserProfile.PersonalityType.Code
            })
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new DiscoveryUsersPageData(totalCount, users);
    }
}
