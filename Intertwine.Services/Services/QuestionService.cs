using Intertwine.Domain.Entities;
using Intertwine.Services.DTOs.Answers;
using Intertwine.Services.DTOs.Categories;
using Intertwine.Services.DTOs.Questions;
using Intertwine.Services.Interfaces;
using Intertwine.Services.Interfaces.Repositories;

namespace Intertwine.Services.Services.Questions;

/// <summary>
/// Retrieves question data and applies Daily Question cache-aside behavior.
/// </summary>
public class QuestionService : IQuestionService
{
    private readonly IQuestionRepository _questionRepository;
    private readonly IDailyQuestionCache _dailyQuestionCache;

    public QuestionService(
        IQuestionRepository questionRepository,
        IDailyQuestionCache dailyQuestionCache)
    {
        _questionRepository = questionRepository;
        _dailyQuestionCache = dailyQuestionCache;
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
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

        return MapToDetailDto(question);
    }

    /// <inheritdoc />
    public async Task<QuestionDetailDto?> GetDailyQuestionAsync(
        DateOnly localDate,
        CancellationToken cancellationToken = default)
    {
        var cachedQuestion = await _dailyQuestionCache.GetAsync(
            localDate,
            cancellationToken);
        if (cachedQuestion != null)
            return cachedQuestion;

        var question = await _questionRepository.GetDailyQuestionByDateAsync(
            localDate,
            cancellationToken);
        if (question == null)
            return null;

        var questionDto = MapToDetailDto(question, localDate);
        await _dailyQuestionCache.SetAsync(
            localDate,
            questionDto,
            cancellationToken);

        return questionDto;
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

    private static QuestionDetailDto MapToDetailDto(
        Question question,
        DateOnly? dailyQuestionDate = null)
    {
        var dailyQuestion = dailyQuestionDate is null
            ? null
            : question.DailyQuestions.FirstOrDefault(x => x.Date == dailyQuestionDate);

        return new QuestionDetailDto
        {
            DailyQuestionId = dailyQuestion?.DailyQuestionId,
            DailyQuestionDate = dailyQuestion?.Date,
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
}
