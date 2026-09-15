namespace Intertwine.Services.Interfaces.Repositories;

public interface IUserPaymentRepository
{
    Task<IReadOnlyList<UserPayment>> GetByUserProfileIdAsync(int userProfileId, int skip, int take,
        CancellationToken cancellationToken = default);
    Task AddAsync(
        UserPayment payment,
        CancellationToken cancellationToken = default);
}
