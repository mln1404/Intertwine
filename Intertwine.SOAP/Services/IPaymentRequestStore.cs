using Intertwine.SOAP.Contracts;

namespace Intertwine.SOAP.Services;

/// <summary>
/// Stores demo provider responses by merchant transaction identifier.
/// </summary>
public interface IPaymentRequestStore
{
    ProcessTopUpResult GetOrAdd(ProcessTopUpRequest request);
}
