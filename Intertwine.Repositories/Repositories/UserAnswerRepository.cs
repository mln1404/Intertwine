using Intertwine.Domain.Entities;
using Intertwine.Repositories.Data;
using Intertwine.Services.Interfaces.Repositories;

namespace Intertwine.Repositories.Repositories;

public class UserAnswerRepository : IUserAnswerRepository
{
    private readonly IntertwineDbContext _context;

    public UserAnswerRepository(IntertwineDbContext context)
    {
        _context = context;
    }

    public async Task<UserAnswers> AddAsync(
        UserAnswers userAnswer,
        CancellationToken cancellationToken = default)
    {
        await _context.UserAnswers.AddAsync(
            userAnswer,
            cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        return userAnswer;
    }
}
