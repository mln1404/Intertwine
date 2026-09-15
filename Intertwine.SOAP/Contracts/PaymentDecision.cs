using System.Runtime.Serialization;

namespace Intertwine.SOAP.Contracts;

/// <summary>
/// Describes the provider's decision for a payment request.
/// </summary>
[DataContract(Namespace = SoapContractNamespaces.PaymentGateway)]
public enum PaymentDecision
{
    [EnumMember]
    Approved = 1,

    [EnumMember]
    Declined = 2
}
