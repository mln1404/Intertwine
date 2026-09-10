using Intertwine.Domain.Entities;

namespace Intertwine.Services.Interfaces;

public interface IUserProfileRepository
{
    Task CreateAsync(UserProfile profile);
}