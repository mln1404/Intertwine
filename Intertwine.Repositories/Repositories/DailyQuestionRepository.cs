using Intertwine.Domain.Entities;
using Intertwine.Repositories.Data;
using Intertwine.Services.DTOs.Questions;
using Intertwine.Services.Interfaces.Repositories;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Intertwine.Repositories.Repositories;

public sealed class DailyQuestionRepository(IntertwineDbContext context) : IDailyQuestionRepository
{
    public Task<bool> ExistsAsync(DateOnly date, CancellationToken cancellationToken = default) =>
        context.DailyQuestions.AnyAsync(q => q.Date == date, cancellationToken);

    public async Task<DailyQuestionSelection> GetSelectionAsync(CancellationToken cancellationToken = default)
    {
        var questions = await context.Questions.AsNoTracking()
            .Where(q => q.IsActive)
            .Select(q => new
            {
                q.QuestionId,
                CategoryIds = q.QuestionCategories.Select(c => c.CategoryId).ToList(),
                LastSelectedDate = q.DailyQuestions.Select(d => (DateOnly?)d.Date).Max()
            })
            .ToListAsync(cancellationToken);

        // Each assignment counts once toward every linked category, including future/reused assignments.
        var categoryLinks = context.QuestionCategories
            .Select(q => new { q.QuestionId, q.CategoryId }).Distinct();
        var usage = await (
            from daily in context.DailyQuestions
            join category in categoryLinks on daily.QuestionId equals category.QuestionId
            group daily by category.CategoryId into assignments
            select new { CategoryId = assignments.Key, Count = assignments.Count() })
            .ToDictionaryAsync(x => x.CategoryId, x => x.Count, cancellationToken);

        return new DailyQuestionSelection(
            questions.Select(q => new DailyQuestionCandidate(q.QuestionId, q.CategoryIds.Distinct().ToList(), q.LastSelectedDate)).ToList(),
            usage);
    }

    public async Task<bool> TryInsertAsync(DailyQuestion assignment, CancellationToken cancellationToken = default)
    {
        context.DailyQuestions.Add(assignment);
        try
        {
            await context.SaveChangesAsync(cancellationToken);
            context.Entry(assignment).State = EntityState.Detached;
            return true;
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 })
        {
            context.Entry(assignment).State = EntityState.Detached;
            // Only treat a competing assignment for this date as an idempotent success.
            if (await ExistsAsync(assignment.Date, cancellationToken))
                return false;
            throw;
        }
    }
}
