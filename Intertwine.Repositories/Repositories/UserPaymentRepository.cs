using Intertwine.Domain.Entities;
using Intertwine.Repositories.Data;
using Intertwine.Services.Interfaces.Repositories;

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
}
