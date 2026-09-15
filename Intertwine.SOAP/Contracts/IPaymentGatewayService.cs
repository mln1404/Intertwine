using CoreWCF;

namespace Intertwine.SOAP.Contracts;

/// <summary>
/// Defines the SOAP contract exposed by the demo payment provider.
/// </summary>
[ServiceContract(Namespace = SoapContractNamespaces.PaymentGateway)]
public interface IPaymentGatewayService
{
    /// <summary>
    /// Processes an idempotent payment request for an Intertwine credit purchase.
    /// </summary>
    [OperationContract]
    Task<ProcessTopUpResult> ProcessTopUpAsync(ProcessTopUpRequest request);
}
