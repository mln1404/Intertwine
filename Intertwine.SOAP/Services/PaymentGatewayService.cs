using CoreWCF;
using Intertwine.SOAP.Authentication;
using Intertwine.SOAP.Contracts;
using Microsoft.AspNetCore.Authorization;

namespace Intertwine.SOAP.Services;

/// <summary>
/// Implements the demo external payment provider's SOAP operations.
/// </summary>
public sealed class PaymentGatewayService : IPaymentGatewayService
{
    private readonly IPaymentRequestStore _paymentRequestStore;

    public PaymentGatewayService(IPaymentRequestStore paymentRequestStore)
    {
        _paymentRequestStore = paymentRequestStore;
    }

    [Authorize(Policy = SoapAuthorizationPolicies.PaymentProvider)]
    public Task<ProcessTopUpResult> ProcessTopUpAsync(
        ProcessTopUpRequest request)
    {
        Validate(request);

        return Task.FromResult(_paymentRequestStore.GetOrAdd(request));
    }

    private static void Validate(ProcessTopUpRequest? request)
    {
        if (request == null)
            throw new FaultException("A payment request is required.");

        if (string.IsNullOrWhiteSpace(request.MerchantTransactionId))
            throw new FaultException("MerchantTransactionId is required.");

        if (string.IsNullOrWhiteSpace(request.PaymentToken))
            throw new FaultException("PaymentToken is required.");

        if (request.Amount <= 0)
            throw new FaultException("Amount must be greater than zero.");

        if (string.IsNullOrWhiteSpace(request.CurrencyCode) ||
            request.CurrencyCode.Length != 3)
            throw new FaultException("CurrencyCode must contain three characters.");
    }
}
