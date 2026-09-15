namespace Intertwine.Services.DTOs.CreditPackages;

public class CreditPackageDto
{
    public int CreditPackageId { get; set; }

    public string CurrencyCode { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public long Credits { get; set; }
}
