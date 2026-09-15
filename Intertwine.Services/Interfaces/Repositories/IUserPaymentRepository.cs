namespace Intertwine.Services.Interfaces.Repositories;

public interface IUserPaymentRepository
{
    Task AddAsync(
        UserPayment payment,
        CancellationToken cancellationToken = default);
}
