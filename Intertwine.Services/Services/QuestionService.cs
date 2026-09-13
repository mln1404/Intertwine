using Intertwine.Domain.Entities;
using Intertwine.Services.DTOs.Answers;
using Intertwine.Services.DTOs.Categories;
using Intertwine.Services.DTOs.Questions;
using Intertwine.Services.DTOs.UserAnswers;
using Intertwine.Services.Interfaces;
using Intertwine.Services.Interfaces.Repositories;

namespace Intertwine.Services.Services.Questions;

public class QuestionService : IQuestionService
{
    private readonly IQuestionRepository _questionRepository;
    private readonly IUserAnswerRepository _userAnswerRepository;
    private readonly IUserProfileRepository _userProfileRepository;

    public QuestionService(
        IQuestionRepository questionRepository,
        IUserAnswerRepository userAnswerRepository,
        IUserProfileRepository userProfileRepository)
    {
        _questionRepository = questionRepository;
        _userAnswerRepository = userAnswerRepository;
        _userProfileRepository = userProfileRepository;
    }

    public async Task<IEnumerable<QuestionListDto>> GetQuestionsAsync(
        int? categoryId = null,
        CancellationToken cancellationToken = default)
    {
        var questions = categoryId.HasValue
            ? await _questionRepository.GetByCategoryAsync(
                categoryId.Value,
                cancellationToken)
            : await _questionRepository.GetAllActiveAsync(
                cancellationToken);

        return questions.Select(MapToListDto);
    }

    public async Task<QuestionDetailDto?> GetQuestionByIdAsync(
        int questionId,
        CancellationToken cancellationToken = default)
    {
        var question =
            await _questionRepository.GetByIdWithAnswersAsync(
                questionId,
                cancellationToken);

        if (question == null)
            return null;

        return new QuestionDetailDto
        {
            QuestionId = question.QuestionId,
            QuestionTitle = question.QuestionTitle,
            FullQuestion = question.FullQuestion,

            Categories = question.QuestionCategories
                .Select(qc => new CategoryDto
                {
                    CategoryId = qc.Category.CategoryId,
                    CategoryName = qc.Category.CategoryName,
                    Color = qc.Category.Color
                })
                .ToList(),

            Answers = question.Answers
                .Where(a => a.IsActive)
                .Select(a => new AnswerDto
                {
                    AnswerId = a.AnswerId,
                    AnswerText = a.AnswerText
                })
                .ToList()
        };
    }

    public async Task<bool> SubmitAnswerAsync(
        string identityUserId,
        int questionId,
        SubmitAnswerRequest request,
        CancellationToken cancellationToken = default)
    {
        var userProfile =
            await _userProfileRepository
                .GetByIdentityUserIdAsync(identityUserId);

        if (userProfile == null)
            throw new InvalidOperationException(
                "User profile not found.");

        var answerBelongsToQuestion =
            await _questionRepository.AnswerBelongsToQuestionAsync(
                request.AnswerId,
                questionId,
                cancellationToken);

        if (!answerBelongsToQuestion)
            throw new ArgumentException(
                "The selected answer does not belong to this question.");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var isDaily =
            await _questionRepository.IsDailyQuestionAsync(
                questionId,
                today,
                cancellationToken);

        if (!isDaily)
        {
            var answerCount =
                await _questionRepository
                    .GetTodayNonDailyAnswerCountAsync(
                        userProfile.UserProfileId,
                        today,
                        cancellationToken);

            if (answerCount >= 2)
                throw new InvalidOperationException(
                    "You can only answer two additional questions per day.");
        }

        var userAnswer = new UserAnswers
        {
            UserProfileId = userProfile.UserProfileId,
            AnswerId = request.AnswerId,
            DateCreated = DateTime.UtcNow,
            CreatedBy = identityUserId
        };

        await _userAnswerRepository.AddAsync(
            userAnswer,
            cancellationToken);

        return true;
    }

    private static QuestionListDto MapToListDto(
        Question question)
    {
        return new QuestionListDto
        {
            QuestionId = question.QuestionId,
            QuestionTitle = question.QuestionTitle,
            FullQuestion = question.FullQuestion,

            Categories = question.QuestionCategories
                .Select(qc => new CategoryDto
                {
                    CategoryId = qc.Category.CategoryId,
                    CategoryName = qc.Category.CategoryName,
                    Color = qc.Category.Color
                })
                .ToList()
        };
    }
}
