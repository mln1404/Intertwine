namespace Intertwine.Services.DTOs.Wallets;

public class TopUpResultDto
{
    public long UserPaymentId { get; set; }
    public long CreditsPurchased { get; set; }
    public long CreditBalance { get; set; }
}