using Intertwine.Domain.Entities;
using Intertwine.Repositories.Data;
using Intertwine.Services.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Intertwine.Repositories.Repositories;

/// <summary>
/// Provides database queries for questions, answers, and Daily Question assignments.
/// </summary>
public class QuestionRepository : IQuestionRepository
{
    private readonly IntertwineDbContext _context;

    public QuestionRepository(IntertwineDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Question>> GetAllActiveAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.Questions
            .AsNoTracking()
            .Where(q => q.IsActive)
            .Include(q => q.QuestionCategories)
                .ThenInclude(qc => qc.Category)
            .OrderBy(q => q.QuestionTitle)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Question>> GetByCategoryAsync(
        int categoryId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Questions
            .AsNoTracking()
            .Where(q =>
                q.IsActive &&
                q.QuestionCategories.Any(qc =>
                    qc.CategoryId == categoryId))
            .Include(q => q.QuestionCategories)
                .ThenInclude(qc => qc.Category)
            .OrderBy(q => q.QuestionTitle)
            .ToListAsync(cancellationToken);
    }

    public async Task<Question?> GetByIdWithAnswersAsync(
        int questionId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Questions
            .AsNoTracking()
            .Where(q =>
                q.QuestionId == questionId &&
                q.IsActive)
            .Include(q => q.QuestionCategories)
                .ThenInclude(qc => qc.Category)
            .Include(q => q.Answers)
            .FirstOrDefaultAsync(
                cancellationToken);
    }

    public async Task<Question?> GetDailyQuestionByDateAsync(
        DateOnly localDate,
        CancellationToken cancellationToken = default)
    {
        return await _context.Questions
            .AsNoTracking()
            .Where(q =>
                q.IsActive &&
                q.DailyQuestions.Any(dq => dq.Date == localDate))
            .Include(q => q.QuestionCategories)
                .ThenInclude(qc => qc.Category)
            .Include(q => q.Answers)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> IsDailyQuestionAsync(
        int questionId,
        DateOnly date,
        CancellationToken cancellationToken = default)
    {
        return await _context.DailyQuestions
            .AnyAsync(
                dq =>
                    dq.QuestionId == questionId &&
                    dq.Date == date,
                cancellationToken);
    }

    public async Task<bool> AnswerBelongsToQuestionAsync(
        int answerId,
        int questionId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Answers
            .AnyAsync(
                a =>
                    a.AnswerId == answerId &&
                    a.QuestionId == questionId,
                cancellationToken);
    }
}
