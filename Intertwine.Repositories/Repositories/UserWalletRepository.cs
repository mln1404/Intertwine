using Intertwine.Domain.Entities;
using Intertwine.Repositories.Data;
using Intertwine.Services.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Intertwine.Repositories.Repositories;

public class UserWalletRepository
    : IUserWalletRepository
{
    private readonly IntertwineDbContext _context;

    public UserWalletRepository(
        IntertwineDbContext context)
    {
        _context = context;
    }

    public async Task<UserWallet?> GetByUserProfileIdAsync(
        int userProfileId,
        CancellationToken cancellationToken = default)
    {
        return await _context.UserWallets
            .FirstOrDefaultAsync(
                x => x.UserProfileId == userProfileId,
                cancellationToken);
    }

    public async Task AddAsync(
        UserWallet wallet,
        CancellationToken cancellationToken = default)
    {
        await _context.UserWallets.AddAsync(
            wallet,
            cancellationToken);
    }
}
