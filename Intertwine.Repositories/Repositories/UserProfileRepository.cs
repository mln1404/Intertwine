using Intertwine.Domain.Entities;
using Intertwine.Repositories.Data;
using Intertwine.Services.Interfaces;

public class UserProfileRepository : IUserProfileRepository
{
    private readonly IntertwineDbContext _context;

    public UserProfileRepository(IntertwineDbContext context)
    {
        _context = context;
    }

    public async Task CreateAsync(UserProfile profile)
    {
        await _context.UserProfiles.AddAsync(profile);
        await _context.SaveChangesAsync();
    }
}