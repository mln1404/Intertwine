using Intertwine.Domain.Entities;
using Intertwine.Repositories.Data;
using Intertwine.Services.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Intertwine.Repositories.Repositories;

/// <summary>
/// Provides tracked access to a user's current answer records.
/// </summary>
public class UserAnswerRepository
    : IUserAnswerRepository
{
    private readonly IntertwineDbContext _context;

    public UserAnswerRepository(
        IntertwineDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<UserAnswers>> GetByUserProfileIdAsync(
        int userProfileId,
        CancellationToken cancellationToken = default)
    {
        return await _context.UserAnswers
            .AsNoTracking()
            .Include(x => x.Answer)
                .ThenInclude(x => x.Question)
                .ThenInclude(x => x.QuestionCategories)
                .ThenInclude(x => x.Category)
            .Where(x => x.UserProfileId == userProfileId)
            .OrderBy(x => x.Answer.QuestionId)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<UserAnswers?> GetByUserAndQuestionAsync(
        int userProfileId,
        int questionId,
        CancellationToken cancellationToken = default)
    {
        return await _context.UserAnswers
            .Include(x => x.Answer)
            .FirstOrDefaultAsync(
                x =>
                    x.UserProfileId == userProfileId &&
                    x.Answer.QuestionId == questionId,
                cancellationToken);
    }

    /// <inheritdoc />
    public async Task<UserAnswers> AddAsync(
        UserAnswers userAnswer,
        CancellationToken cancellationToken = default)
    {
        await _context.UserAnswers.AddAsync(
            userAnswer,
            cancellationToken);

        return userAnswer;
    }
}
