using Intertwine.Domain.Entities;
using Intertwine.Services.Constants;
using Intertwine.Services.DTOs.UserDailyActivities;
using Intertwine.Services.Interfaces;
using Intertwine.Services.Interfaces.Repositories;

namespace Intertwine.Services.Services;

/// <summary>
/// Provides the authenticated user's answer usage for a local date.
/// </summary>
public class UserDailyActivityService : IUserDailyActivityService
{
    private readonly IUserProfileRepository _userProfileRepository;
    private readonly IUserDailyActivityRepository _activityRepository;

    public UserDailyActivityService(
        IUserProfileRepository userProfileRepository,
        IUserDailyActivityRepository activityRepository)
    {
        _userProfileRepository = userProfileRepository;
        _activityRepository = activityRepository;
    }

    /// <inheritdoc />
    public async Task<UserDailyActivityDto> GetCurrentUserActivityAsync(
        string identityUserId,
        DateOnly localDate,
        CancellationToken cancellationToken = default)
    {
        var userProfile = await _userProfileRepository
            .GetByIdentityUserIdAsync(identityUserId);

        if (userProfile is null)
        {
            throw new InvalidOperationException(
                UserAnswerMessages.UserProfileNotFound);
        }

        var activity = await _activityRepository.GetByUserAndDateAsync(
            userProfile.UserProfileId,
            localDate,
            cancellationToken);

        return MapToDto(localDate, activity);
    }

    private static UserDailyActivityDto MapToDto(
        DateOnly localDate,
        UserDailyActivity? activity)
    {
        var nonDailyQuestionsAnswered =
            activity?.NonDailyQuestionsAnswered ?? 0;

        return new UserDailyActivityDto
        {
            LocalDate = localDate,
            DailyQuestionCreateOrUpdateUsed =
                activity?.DailyQuestionCreateOrUpdateUsed ?? false,
            NonDailyQuestionsAnswered = nonDailyQuestionsAnswered,
            NonDailyQuestionsRemaining = Math.Max(
                0,
                UserAnswerLimits.MaxNonDailyQuestionsPerDay -
                    nonDailyQuestionsAnswered)
        };
    }
}
