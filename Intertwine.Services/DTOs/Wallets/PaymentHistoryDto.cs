namespace Intertwine.Services.DTOs.Wallets;

/// <summary>A purchase snapshot belonging to the authenticated user.</summary>
public class PaymentHistoryDto
{
    public long UserPaymentId { get; set; }
    public DateTime DateCreated { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public long SparksPurchased { get; set; }
    public string Status { get; set; } = string.Empty;
    public string PaymentProvider { get; set; } = string.Empty;
}
