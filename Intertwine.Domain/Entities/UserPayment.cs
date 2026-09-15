using Intertwine.Domain.Abstractions;
using Intertwine.Domain.Entities;
using Intertwine.Domain.Enums;

public class UserPayment : BaseEntity
{
    public long UserPaymentId { get; set; }

    public int UserProfileId { get; set; }
    public UserProfile UserProfile { get; set; } = null!;

    public int CurrencyId { get; set; }
    public Currency Currency { get; set; } = null!;

    public decimal Amount { get; set; }

    public long CreditsPurchased { get; set; }

    public string PaymentProvider { get; set; } = string.Empty;
    public string? ProviderTransactionId { get; set; }

    public PaymentStatus Status { get; set; }

    public byte[] RowVersion { get; set; } = [];
}