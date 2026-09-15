using Intertwine.Domain.Entities;
using Intertwine.Services.Constants;
using Intertwine.Services.DTOs.UserAnswers;
using Intertwine.Services.Interfaces.Repositories;
using Moq;
using UserAnswerServiceUnderTest = Intertwine.Services.Services.UserAnswerService;

namespace Intertwine.UnitTests.Services.UserAnswerService;

/// <summary>
/// Unit tests for the answer-submission business rules.
/// </summary>
public class UserAnswerServiceTests
{
    private const string IdentityUserId = "identity-user-id";
    private const int UserProfileId = 1;
    private const int QuestionId = 10;
    private const int AnswerId = 100;
    private static readonly DateOnly LocalDate = new(2026, 9, 14);

    [Fact]
    public async Task GetCurrentAnswersAsync_WhenAnswersExist_ReturnsQuestionAnswerPairs()
    {
        var context = CreateContext();
        context.UserAnswers.Setup(x => x.GetByUserProfileIdAsync(
                UserProfileId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([
                new UserAnswers
                {
                    AnswerId = AnswerId,
                    Answer = new Answer
                    {
                        AnswerId = AnswerId,
                        QuestionId = QuestionId
                    }
                }
            ]);

        var result = await context.Service.GetCurrentAnswersAsync(
            IdentityUserId);

        var answer = Assert.Single(result);
        Assert.Equal(QuestionId, answer.QuestionId);
        Assert.Equal(AnswerId, answer.AnswerId);
    }

    [Fact]
    public async Task GetCurrentAnswersAsync_WhenProfileDoesNotExist_Throws()
    {
        var context = CreateContext();
        context.UserProfiles.Setup(x =>
                x.GetByIdentityUserIdAsync(IdentityUserId))
            .ReturnsAsync((UserProfile?)null);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => context.Service.GetCurrentAnswersAsync(IdentityUserId));

        Assert.Equal(UserAnswerMessages.UserProfileNotFound, exception.Message);
        context.UserAnswers.Verify(x => x.GetByUserProfileIdAsync(
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task SubmitAnswerAsync_FirstDailyAnswer_CreatesAnswerAndConsumesDailyAction()
    {
        var context = CreateContext(isDailyQuestion: true);

        await context.Service.SubmitAnswerAsync(IdentityUserId, QuestionId, CreateRequest(), LocalDate);

        context.UserAnswers.Verify(x => x.AddAsync(
            It.Is<UserAnswers>(answer => answer.UserProfileId == UserProfileId && answer.AnswerId == AnswerId),
            It.IsAny<CancellationToken>()), Times.Once);
        context.DailyActivities.Verify(x => x.AddAsync(
            It.Is<UserDailyActivity>(activity => activity.DailyQuestionCreateOrUpdateUsed && activity.NonDailyQuestionsAnswered == 0),
            It.IsAny<CancellationToken>()), Times.Once);
        VerifySavedOnce(context);
    }

    [Fact]
    public async Task SubmitAnswerAsync_SecondDailyAction_ThrowsAndDoesNotSave()
    {
        var context = CreateContext(isDailyQuestion: true);
        context.DailyActivities.Setup(x => x.GetByUserAndDateAsync(UserProfileId, LocalDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateActivity(dailyQuestionActionUsed: true));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            context.Service.SubmitAnswerAsync(IdentityUserId, QuestionId, CreateRequest(), LocalDate));

        Assert.Equal(UserAnswerMessages.DailyQuestionAlreadyAnswered, exception.Message);
        VerifyNoAnswerOrActivityWrite(context);
        VerifyNotSaved(context);
    }

    [Fact]
    public async Task SubmitAnswerAsync_FirstDailyActionWithExistingAnswer_UpdatesAnswerAndConsumesDailyAction()
    {
        var context = CreateContext(isDailyQuestion: true);
        var existingAnswer = new UserAnswers { UserProfileId = UserProfileId, AnswerId = 99 };
        var activity = CreateActivity();
        context.UserAnswers.Setup(x => x.GetByUserAndQuestionAsync(UserProfileId, QuestionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingAnswer);
        context.DailyActivities.Setup(x => x.GetByUserAndDateAsync(UserProfileId, LocalDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(activity);

        await context.Service.SubmitAnswerAsync(IdentityUserId, QuestionId, CreateRequest(), LocalDate);

        Assert.Equal(AnswerId, existingAnswer.AnswerId);
        Assert.True(activity.DailyQuestionCreateOrUpdateUsed);
        context.UserAnswers.Verify(x => x.AddAsync(It.IsAny<UserAnswers>(), It.IsAny<CancellationToken>()), Times.Never);
        VerifySavedOnce(context);
    }

    [Fact]
    public async Task SubmitAnswerAsync_NewNonDailyAnswer_CreatesAnswerAndConsumesSlot()
    {
        var context = CreateContext();

        await context.Service.SubmitAnswerAsync(IdentityUserId, QuestionId, CreateRequest(), LocalDate);

        context.UserAnswers.Verify(x => x.AddAsync(
            It.Is<UserAnswers>(answer => answer.AnswerId == AnswerId),
            It.IsAny<CancellationToken>()), Times.Once);
        context.DailyActivities.Verify(x => x.AddAsync(
            It.Is<UserDailyActivity>(activity => activity.NonDailyQuestionsAnswered == 1 && !activity.DailyQuestionCreateOrUpdateUsed),
            It.IsAny<CancellationToken>()), Times.Once);
        VerifySavedOnce(context);
    }

    [Fact]
    public async Task SubmitAnswerAsync_UpdatedNonDailyAnswer_ConsumesSlot()
    {
        var context = CreateContext();
        var existingAnswer = new UserAnswers { UserProfileId = UserProfileId, AnswerId = 99 };
        var activity = CreateActivity(nonDailyQuestionsAnswered: 1);
        context.UserAnswers.Setup(x => x.GetByUserAndQuestionAsync(UserProfileId, QuestionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingAnswer);
        context.DailyActivities.Setup(x => x.GetByUserAndDateAsync(UserProfileId, LocalDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(activity);

        await context.Service.SubmitAnswerAsync(IdentityUserId, QuestionId, CreateRequest(), LocalDate);

        Assert.Equal(AnswerId, existingAnswer.AnswerId);
        Assert.Equal(2, activity.NonDailyQuestionsAnswered);
        VerifySavedOnce(context);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task SubmitAnswerAsync_NonDailyLimitReached_ThrowsAndDoesNotSave(bool hasExistingAnswer)
    {
        var context = CreateContext();
        context.DailyActivities.Setup(x => x.GetByUserAndDateAsync(UserProfileId, LocalDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateActivity(nonDailyQuestionsAnswered: 2));
        context.UserAnswers.Setup(x => x.GetByUserAndQuestionAsync(UserProfileId, QuestionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(hasExistingAnswer ? new UserAnswers { UserProfileId = UserProfileId, AnswerId = 99 } : null);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            context.Service.SubmitAnswerAsync(IdentityUserId, QuestionId, CreateRequest(), LocalDate));

        Assert.Equal(UserAnswerMessages.NonDailyQuestionLimitReached, exception.Message);
        VerifyNoAnswerOrActivityWrite(context);
        VerifyNotSaved(context);
    }

    [Fact]
    public async Task SubmitAnswerAsync_SameNonDailyAnswer_ThrowsWithoutConsumingSlot()
    {
        var context = CreateContext();
        context.UserAnswers.Setup(x => x.GetByUserAndQuestionAsync(UserProfileId, QuestionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserAnswers { UserProfileId = UserProfileId, AnswerId = AnswerId });

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            context.Service.SubmitAnswerAsync(IdentityUserId, QuestionId, CreateRequest(), LocalDate));

        Assert.Equal(UserAnswerMessages.ExistingNonDailyAnswer, exception.Message);
        context.DailyActivities.Verify(x => x.GetByUserAndDateAsync(It.IsAny<int>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()), Times.Never);
        VerifyNotSaved(context);
    }

    [Fact]
    public async Task SubmitAnswerAsync_AnswerDoesNotBelongToQuestion_ThrowsBeforeAnyWrite()
    {
        var context = CreateContext();
        context.Questions.Setup(x => x.AnswerBelongsToQuestionAsync(AnswerId, QuestionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            context.Service.SubmitAnswerAsync(IdentityUserId, QuestionId, CreateRequest(), LocalDate));

        Assert.Equal(UserAnswerMessages.AnswerDoesNotBelongToQuestion, exception.Message);
        VerifyNoAnswerOrActivityWrite(context);
        VerifyNotSaved(context);
    }

    [Fact]
    public async Task SubmitAnswerAsync_UserProfileDoesNotExist_ThrowsBeforeAnyWrite()
    {
        var context = CreateContext();
        context.UserProfiles.Setup(x => x.GetByIdentityUserIdAsync(IdentityUserId)).ReturnsAsync((UserProfile?)null);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            context.Service.SubmitAnswerAsync(IdentityUserId, QuestionId, CreateRequest(), LocalDate));

        Assert.Equal(UserAnswerMessages.UserProfileNotFound, exception.Message);
        VerifyNoAnswerOrActivityWrite(context);
        VerifyNotSaved(context);
    }

    private static UserAnswerServiceContext CreateContext(bool isDailyQuestion = false)
    {
        var questions = new Mock<IQuestionRepository>();
        var userAnswers = new Mock<IUserAnswerRepository>();
        var dailyActivities = new Mock<IUserDailyActivityRepository>();
        var userProfiles = new Mock<IUserProfileRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        questions.Setup(x => x.AnswerBelongsToQuestionAsync(AnswerId, QuestionId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        questions.Setup(x => x.IsDailyQuestionAsync(QuestionId, LocalDate, It.IsAny<CancellationToken>())).ReturnsAsync(isDailyQuestion);
        userAnswers.Setup(x => x.GetByUserAndQuestionAsync(UserProfileId, QuestionId, It.IsAny<CancellationToken>())).ReturnsAsync((UserAnswers?)null);
        userAnswers.Setup(x => x.AddAsync(It.IsAny<UserAnswers>(), It.IsAny<CancellationToken>())).ReturnsAsync((UserAnswers answer, CancellationToken _) => answer);
        dailyActivities.Setup(x => x.GetByUserAndDateAsync(UserProfileId, LocalDate, It.IsAny<CancellationToken>())).ReturnsAsync((UserDailyActivity?)null);
        dailyActivities.Setup(x => x.AddAsync(It.IsAny<UserDailyActivity>(), It.IsAny<CancellationToken>())).ReturnsAsync((UserDailyActivity activity, CancellationToken _) => activity);
        userProfiles.Setup(x => x.GetByIdentityUserIdAsync(IdentityUserId)).ReturnsAsync(new UserProfile { UserProfileId = UserProfileId, IdentityUserId = IdentityUserId });
        unitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        return new UserAnswerServiceContext(questions, userAnswers, dailyActivities, userProfiles, unitOfWork);
    }

    private static SubmitAnswerRequest CreateRequest() => new() { AnswerId = AnswerId };

    private static UserDailyActivity CreateActivity(int nonDailyQuestionsAnswered = 0, bool dailyQuestionActionUsed = false) =>
        new()
        {
            UserProfileId = UserProfileId,
            Date = LocalDate,
            NonDailyQuestionsAnswered = nonDailyQuestionsAnswered,
            DailyQuestionCreateOrUpdateUsed = dailyQuestionActionUsed
        };

    private static void VerifyNoAnswerOrActivityWrite(UserAnswerServiceContext context)
    {
        context.UserAnswers.Verify(x => x.AddAsync(It.IsAny<UserAnswers>(), It.IsAny<CancellationToken>()), Times.Never);
        context.DailyActivities.Verify(x => x.AddAsync(It.IsAny<UserDailyActivity>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private static void VerifyNotSaved(UserAnswerServiceContext context) =>
        context.UnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);

    private static void VerifySavedOnce(UserAnswerServiceContext context) =>
        context.UnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

    private sealed class UserAnswerServiceContext
    {
        public UserAnswerServiceContext(
            Mock<IQuestionRepository> questions,
            Mock<IUserAnswerRepository> userAnswers,
            Mock<IUserDailyActivityRepository> dailyActivities,
            Mock<IUserProfileRepository> userProfiles,
            Mock<IUnitOfWork> unitOfWork)
        {
            Questions = questions;
            UserAnswers = userAnswers;
            DailyActivities = dailyActivities;
            UserProfiles = userProfiles;
            UnitOfWork = unitOfWork;
            Service = new UserAnswerServiceUnderTest(
                Questions.Object,
                UserAnswers.Object,
                DailyActivities.Object,
                UserProfiles.Object,
                UnitOfWork.Object);
        }

        public Mock<IQuestionRepository> Questions { get; }
        public Mock<IUserAnswerRepository> UserAnswers { get; }
        public Mock<IUserDailyActivityRepository> DailyActivities { get; }
        public Mock<IUserProfileRepository> UserProfiles { get; }
        public Mock<IUnitOfWork> UnitOfWork { get; }
        public UserAnswerServiceUnderTest Service { get; }
    }
}
