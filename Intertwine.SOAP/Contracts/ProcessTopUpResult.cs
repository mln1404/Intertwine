using System.Runtime.Serialization;

namespace Intertwine.SOAP.Contracts;

/// <summary>
/// Reports the provider result of a credit-purchase payment request.
/// </summary>
[DataContract(Namespace = SoapContractNamespaces.PaymentGateway)]
public sealed class ProcessTopUpResult
{
    [DataMember(Order = 1)]
    public string MerchantTransactionId { get; set; } = string.Empty;

    [DataMember(Order = 2)]
    public string ProviderTransactionId { get; set; } = string.Empty;

    [DataMember(Order = 3)]
    public PaymentDecision Decision { get; set; }

    [DataMember(Order = 4)]
    public DateTime ProcessedAtUtc { get; set; }

    [DataMember(Order = 5)]
    public string? DeclineReason { get; set; }
}
