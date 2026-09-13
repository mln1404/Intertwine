using Intertwine.Domain.Entities;
using Intertwine.Services.DTOs.Answers;
using Intertwine.Services.DTOs.Categories;
using Intertwine.Services.DTOs.Questions;
using Intertwine.Services.Interfaces;
using Intertwine.Services.Interfaces.Repositories;

namespace Intertwine.Services.Services.Questions;

public class QuestionService : IQuestionService
{
    private readonly IQuestionRepository _questionRepository;

    public QuestionService(IQuestionRepository questionRepository)
    {
        _questionRepository = questionRepository;
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
