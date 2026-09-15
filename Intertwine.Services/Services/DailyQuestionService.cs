using Intertwine.Domain.Entities;
using Intertwine.Services.DTOs.Questions;
using Intertwine.Services.Interfaces;
using Intertwine.Services.Interfaces.Repositories;

namespace Intertwine.Services.Services;

/// <summary>Assigns questions to dates using category balance, then least-recent reuse.</summary>
public sealed class DailyQuestionService(
    IDailyQuestionRepository repository,
    TimeProvider timeProvider) : IDailyQuestionService
{
    public async Task<EnsureDailyQuestionsResult> EnsureDailyQuestionsAsync(
        DateOnly startDate,
        int daysAhead = 7,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(daysAhead);
        var endDate = startDate.AddDays(daysAhead);
        var created = 0;
        var existing = 0;

        for (var offset = 0; offset <= daysAhead; offset++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var date = startDate.AddDays(offset);

            if (await repository.ExistsAsync(date, cancellationToken))
            {
                existing++;
            }
            else
            {
                // Refresh after each insert so future dates and concurrent writes contribute to selection.
                var selection = await repository.GetSelectionAsync(cancellationToken);
                var question = SelectQuestion(selection)
                    ?? throw new InvalidOperationException("No active questions are available for Daily Question assignment.");

                var assignment = new DailyQuestion
                {
                    Date = date,
                    QuestionId = question.QuestionId,
                    DateCreated = timeProvider.GetUtcNow().UtcDateTime,
                    CreatedBy = "DailyQuestionWorker"
                };

                if (await repository.TryInsertAsync(assignment, cancellationToken))
                    created++;
                else
                    existing++;
            }

        }

        return new EnsureDailyQuestionsResult(startDate, endDate, created, existing);
    }

    private static DailyQuestionCandidate? SelectQuestion(DailyQuestionSelection selection)
    {
        var unused = selection.Candidates.Where(q => q.LastSelectedDate is null).ToList();
        var categorized = unused
            .SelectMany(q => q.CategoryIds.Distinct().Select(categoryId => new { Question = q, CategoryId = categoryId }))
            .OrderBy(x => selection.CategoryUsage.GetValueOrDefault(x.CategoryId))
            .ThenBy(x => x.CategoryId)
            .ThenBy(x => x.Question.QuestionId)
            .FirstOrDefault();

        return categorized?.Question
            // An uncategorized active question is still unused and precedes any reuse.
            ?? unused.OrderBy(q => q.QuestionId).FirstOrDefault()
            ?? selection.Candidates.OrderBy(q => q.LastSelectedDate).ThenBy(q => q.QuestionId).FirstOrDefault();
    }
}
