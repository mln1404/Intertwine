using Intertwine.Domain.Abstractions;
using Intertwine.Domain.Enums;

public class FinancialTransaction : BaseEntity
{
    public long FinancialTransactionId { get; set; }

    public int UserWalletId { get; set; }
    public UserWallet UserWallet { get; set; } = null!;

    public long CreditAmount { get; set; }

    public FinancialTransactionType TransactionType { get; set; }

    public long? UserPaymentId { get; set; }
    public UserPayment? UserPayment { get; set; }

    public long BalanceAfterTransaction { get; set; }

    public string? Description { get; set; }
}