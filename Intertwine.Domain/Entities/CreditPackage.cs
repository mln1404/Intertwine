using Intertwine.Domain.Abstractions;

public class CreditPackage : ActivatableEntity
{
    public int CreditPackageId { get; set; }

    public int CurrencyId { get; set; }
    public Currency Currency { get; set; } = null!;

    public decimal Amount { get; set; }

    public long Credits { get; set; }
}