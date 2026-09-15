using System.Runtime.Serialization;

namespace Intertwine.SOAP.Contracts;

/// <summary>
/// Contains the payment data needed to purchase Intertwine credits.
/// </summary>
[DataContract(Namespace = SoapContractNamespaces.PaymentGateway)]
public sealed class ProcessTopUpRequest
{
    [DataMember(Order = 1, IsRequired = true)]
    public string MerchantTransactionId { get; set; } = string.Empty;

    [DataMember(Order = 2, IsRequired = true)]
    public string PaymentToken { get; set; } = string.Empty;

    [DataMember(Order = 3, IsRequired = true)]
    public decimal Amount { get; set; }

    [DataMember(Order = 4, IsRequired = true)]
    public string CurrencyCode { get; set; } = string.Empty;
}
