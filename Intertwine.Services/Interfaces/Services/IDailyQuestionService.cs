using Intertwine.Services.DTOs.Questions;

namespace Intertwine.Services.Interfaces.Services;

public interface IDailyQuestionService
{
    /// <summary>Preserves existing assignments and fills the inclusive date range.</summary>
    Task<EnsureDailyQuestionsResult> EnsureDailyQuestionsAsync(
        DateOnly startDate,
        int daysAhead = 7,
        CancellationToken cancellationToken = default);
}
