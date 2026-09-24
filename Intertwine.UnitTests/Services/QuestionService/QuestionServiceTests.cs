using Intertwine.Domain.Entities;
using Intertwine.Services.DTOs.Questions;
using Intertwine.Services.Interfaces.Repositories;
using Moq;
using QuestionServiceUnderTest = Intertwine.Services.Services.Questions.QuestionService;

namespace Intertwine.UnitTests.Services.QuestionService;

public class QuestionServiceTests
{
    private static readonly DateOnly LocalDate = new(2026, 9, 15);

    [Fact]
    public async Task GetDailyQuestionAsync_WhenCached_ReturnsCachedQuestionWithoutDatabaseQuery()
    {
        var repository = new Mock<IQuestionRepository>(MockBehavior.Strict);
        var cache = new Mock<IDailyQuestionCache>(MockBehavior.Strict);
        var cachedQuestion = new QuestionDetailDto
        {
            QuestionId = 10,
            QuestionTitle = "Cached question"
        };

        cache.Setup(x => x.GetAsync(LocalDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedQuestion);

        var service = new QuestionServiceUnderTest(repository.Object, cache.Object);

        var result = await service.GetDailyQuestionAsync(LocalDate);

        Assert.Same(cachedQuestion, result);
        repository.Verify(
            x => x.GetDailyQuestionByDateAsync(
                It.IsAny<DateOnly>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
        cache.Verify(
            x => x.SetAsync(
                It.IsAny<DateOnly>(),
                It.IsAny<QuestionDetailDto>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetDailyQuestionAsync_WhenNotCached_QueriesDatabaseAndCachesMappedQuestion()
    {
        var repository = new Mock<IQuestionRepository>(MockBehavior.Strict);
        var cache = new Mock<IDailyQuestionCache>(MockBehavior.Strict);
        var question = CreateQuestion();

        cache.Setup(x => x.GetAsync(LocalDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync((QuestionDetailDto?)null);
        repository.Setup(x => x.GetDailyQuestionByDateAsync(
                LocalDate,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(question);
        cache.Setup(x => x.SetAsync(
                LocalDate,
                It.Is<QuestionDetailDto>(dto =>
                    dto.DailyQuestionId == 40 &&
                    dto.DailyQuestionDate == LocalDate &&
                    dto.QuestionId == question.QuestionId &&
                    dto.Categories.Single().CategoryName == "Culture" &&
                    dto.Answers.Single().AnswerText == "Coffee"),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = new QuestionServiceUnderTest(repository.Object, cache.Object);

        var result = await service.GetDailyQuestionAsync(LocalDate);

        Assert.NotNull(result);
        Assert.Equal(question.QuestionId, result.QuestionId);
        Assert.Equal(40, result.DailyQuestionId);
        Assert.Equal(LocalDate, result.DailyQuestionDate);
        Assert.Single(result.Categories);
        Assert.Single(result.Answers);
        cache.VerifyAll();
        repository.VerifyAll();
    }

    [Fact]
    public async Task GetDailyQuestionAsync_WhenNoQuestionIsAssigned_ReturnsNullWithoutCaching()
    {
        var repository = new Mock<IQuestionRepository>(MockBehavior.Strict);
        var cache = new Mock<IDailyQuestionCache>(MockBehavior.Strict);

        cache.Setup(x => x.GetAsync(LocalDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync((QuestionDetailDto?)null);
        repository.Setup(x => x.GetDailyQuestionByDateAsync(
                LocalDate,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Question?)null);

        var service = new QuestionServiceUnderTest(repository.Object, cache.Object);

        var result = await service.GetDailyQuestionAsync(LocalDate);

        Assert.Null(result);
        cache.Verify(
            x => x.SetAsync(
                It.IsAny<DateOnly>(),
                It.IsAny<QuestionDetailDto>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private static Question CreateQuestion() => new()
    {
        QuestionId = 20,
        QuestionTitle = "Morning ritual",
        FullQuestion = "Which drink starts your day?",
        QuestionCategories =
        [
            new QuestionCategories
            {
                Category = new Category
                {
                    CategoryId = 3,
                    CategoryName = "Culture",
                    Color = "#123456"
                }
            }
        ],
        Answers =
        [
            new Answer
            {
                AnswerId = 30,
                AnswerText = "Coffee",
                IsActive = true
            },
            new Answer
            {
                AnswerId = 31,
                AnswerText = "Inactive answer",
                IsActive = false
            }
        ],
        DailyQuestions =
        [
            new DailyQuestion
            {
                DailyQuestionId = 40,
                Date = LocalDate
            }
        ]
    };
}
