using Intertwine.Domain.Abstractions;
using Intertwine.Domain.Entities;
using Intertwine.Services.DTOs.UserAnswers;
using Intertwine.Services.Interfaces.Repositories;
using Intertwine.Services.Interfaces.Services;

namespace Intertwine.Services.Services
{
    public class UserAnswerService : IUserAnswerService
    {
        private readonly IQuestionRepository _questionRepository;
        private readonly IUserAnswerRepository _userAnswerRepository;
        private readonly IUserDailyActivityRepository _userDailyActivityRepository;
        private readonly IUserProfileRepository _userProfileRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UserAnswerService(
            IQuestionRepository questionRepository,
            IUserAnswerRepository userAnswerRepository,
            IUserDailyActivityRepository userDailyActivityRepository,
            IUserProfileRepository userProfileRepository,
            IUnitOfWork unitOfWork)
        {
            _questionRepository = questionRepository;
            _userAnswerRepository = userAnswerRepository;
            _userDailyActivityRepository = userDailyActivityRepository;
            _userProfileRepository = userProfileRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task SubmitAnswerAsync(
            string identityUserId,
            int questionId,
            SubmitAnswerRequest request,
            DateOnly localDate,
            CancellationToken cancellationToken = default)
        {
            var userProfile = await GetUserProfileAsync(identityUserId, cancellationToken);

            await ValidateAnswerAsync(
                questionId,
                request.AnswerId,
                cancellationToken);

            var isDailyQuestion =
                await _questionRepository.IsDailyQuestionAsync(
                    questionId,
                    localDate,
                    cancellationToken);

            var existingUserAnswer =
                await _userAnswerRepository.GetByUserAndQuestionAsync(
                    userProfile.UserProfileId,
                    questionId,
                    cancellationToken);

            if (isDailyQuestion)
            {
                await SubmitDailyQuestionAnswerAsync(
                    userProfile.UserProfileId,
                    existingUserAnswer,
                    request.AnswerId,
                    localDate,
                    identityUserId,
                    cancellationToken);
            }
            else if (existingUserAnswer != null)
            {
                UpdateNonDailyAnswer(
                    existingUserAnswer,
                    request.AnswerId,
                    identityUserId);
            }
            else
            {
                await CreateNonDailyAnswerAsync(
                    userProfile.UserProfileId,
                    request.AnswerId,
                    localDate,
                    identityUserId,
                    cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        private async Task<UserProfile> GetUserProfileAsync(
            string identityUserId,
            CancellationToken cancellationToken)
        {
            var userProfile =
                await _userProfileRepository.GetByIdentityUserIdAsync(
                    identityUserId);

            if (userProfile == null)
            {
                throw new InvalidOperationException(
                    "User profile not found.");
            }

            return userProfile;
        }

        private async Task ValidateAnswerAsync(
            int questionId,
            int answerId,
            CancellationToken cancellationToken)
        {
            var belongsToQuestion =
                await _questionRepository.AnswerBelongsToQuestionAsync(
                    answerId,
                    questionId,
                    cancellationToken);

            if (!belongsToQuestion)
            {
                throw new ArgumentException(
                    "The selected answer does not belong to this question.");
            }
        }

        private static void UpdateNonDailyAnswer(
            UserAnswers existingUserAnswer,
            int newAnswerId,
            string identityUserId)
        {
            if (existingUserAnswer.AnswerId == newAnswerId)
            {
                throw new InvalidOperationException(
                    "You have already selected this answer for this question.");
            }

            UpdateUserAnswer(existingUserAnswer, newAnswerId, identityUserId);
        }

        private async Task CreateNonDailyAnswerAsync(
            int userProfileId,
            int answerId,
            DateOnly localDate,
            string identityUserId,
            CancellationToken cancellationToken)
        {
            var activity =
                await GetOrCreateDailyActivityAsync(
                    userProfileId,
                    localDate,
                    identityUserId,
                    cancellationToken);

            if (activity.NonDailyQuestionsAnswered >= 2)
            {
                throw new InvalidOperationException(
                    "You have already answered two additional questions today.");
            }

            activity.NonDailyQuestionsAnswered++;
            UpdateAuditFields(activity, identityUserId);

            var userAnswer = new UserAnswers
            {
                UserProfileId = userProfileId,
                AnswerId = answerId,
                DateCreated = DateTime.UtcNow,
                CreatedBy = identityUserId
            };

            await _userAnswerRepository.AddAsync(
                userAnswer,
                cancellationToken);
        }

        private async Task SubmitDailyQuestionAnswerAsync(
            int userProfileId,
            UserAnswers? existingUserAnswer,
            int newAnswerId,
            DateOnly localDate,
            string identityUserId,
            CancellationToken cancellationToken)
        {
            var activity =
                await GetOrCreateDailyActivityAsync(
                    userProfileId,
                    localDate,
                    identityUserId,
                    cancellationToken);

            if (activity.DailyQuestionCreateOrUpdateUsed)
            {
                throw new InvalidOperationException(
                    "You have already answered today's Daily Question.");
            }

            if (existingUserAnswer == null)
            {
                await _userAnswerRepository.AddAsync(
                    new UserAnswers
                    {
                        UserProfileId = userProfileId,
                        AnswerId = newAnswerId,
                        DateCreated = DateTime.UtcNow,
                        CreatedBy = identityUserId
                    },
                    cancellationToken);
            }
            else if (existingUserAnswer.AnswerId != newAnswerId)
            {
                UpdateUserAnswer(existingUserAnswer, newAnswerId, identityUserId);
            }

            activity.DailyQuestionCreateOrUpdateUsed = true;
            UpdateAuditFields(activity, identityUserId);
        }

        private async Task<UserDailyActivity> GetOrCreateDailyActivityAsync(
            int userProfileId,
            DateOnly localDate,
            string identityUserId,
            CancellationToken cancellationToken)
        {
            var activity =
                await _userDailyActivityRepository.GetByUserAndDateAsync(
                    userProfileId,
                    localDate,
                    cancellationToken);

            if (activity != null)
            {
                return activity;
            }

            activity = new UserDailyActivity
            {
                UserProfileId = userProfileId,
                Date = localDate,
                NonDailyQuestionsAnswered = 0,
                DailyQuestionCreateOrUpdateUsed = false,
                DateCreated = DateTime.UtcNow,
                CreatedBy = identityUserId
            };

            return await _userDailyActivityRepository.AddAsync(
                activity,
                cancellationToken);
        }

        private static void UpdateUserAnswer(
            UserAnswers userAnswer,
            int newAnswerId,
            string identityUserId)
        {
            userAnswer.AnswerId = newAnswerId;
            userAnswer.DateUpdated = DateTime.UtcNow;
            userAnswer.UpdatedBy = identityUserId;
        }

        private static void UpdateAuditFields(
            BaseEntity entity,
            string identityUserId)
        {
            entity.DateUpdated = DateTime.UtcNow;
            entity.UpdatedBy = identityUserId;
        }
    }
}
