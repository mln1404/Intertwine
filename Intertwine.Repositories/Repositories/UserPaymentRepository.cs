using Intertwine.Domain.Entities;
using Intertwine.Repositories.Data;
using Intertwine.Services.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Intertwine.Repositories.Repositories;

public class UserPaymentRepository
    : IUserPaymentRepository
{
    private readonly IntertwineDbContext _context;

    public UserPaymentRepository(
        IntertwineDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(
        UserPayment payment,
        CancellationToken cancellationToken = default)
    {
        await _context.UserPayments.AddAsync(
            payment,
            cancellationToken);
    }

    public async Task<IReadOnlyList<UserPayment>> GetByUserProfileIdAsync(int userProfileId, int skip, int take,
        CancellationToken cancellationToken = default) =>
        await _context.UserPayments.AsNoTracking()
            .Where(x => x.UserProfileId == userProfileId)
            .OrderByDescending(x => x.DateCreated).ThenByDescending(x => x.UserPaymentId)
            .Skip(skip).Take(take).ToListAsync(cancellationToken);
}
