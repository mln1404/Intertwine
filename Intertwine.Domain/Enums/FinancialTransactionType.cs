namespace Intertwine.Domain.Enums;

/// <summary>
/// Identifies whether a financial ledger entry adds or spends Sparks.
/// </summary>
public enum FinancialTransactionType
{
    CreditPurchase = 1,
    CreditSpend = 2,
    Refund = 3,
    PromotionalCredit = 4,
    Adjustment = 5
}
