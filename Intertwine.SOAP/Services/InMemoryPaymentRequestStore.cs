using System.Collections.Concurrent;
using CoreWCF;
using Intertwine.SOAP.Contracts;

namespace Intertwine.SOAP.Services;

/// <summary>
/// Simulates an idempotent external payment provider for local development.
/// </summary>
public sealed class InMemoryPaymentRequestStore : IPaymentRequestStore
{
    private const string DeclinedToken = "tok_declined";

    private readonly ConcurrentDictionary<string, StoredPaymentRequest> _requests =
        new(StringComparer.Ordinal);

    public ProcessTopUpResult GetOrAdd(ProcessTopUpRequest request)
    {
        var proposedRequest = CreateStoredRequest(request);
        var storedRequest = _requests.GetOrAdd(
            request.MerchantTransactionId,
            proposedRequest);

        if (!HasSamePaymentDetails(storedRequest, request))
        {
            throw new FaultException(
                "The merchant transaction ID was already used for a different payment request.");
        }

        return storedRequest.Response;
    }

    private static StoredPaymentRequest CreateStoredRequest(
        ProcessTopUpRequest request)
    {
        var isDeclined = string.Equals(
            request.PaymentToken,
            DeclinedToken,
            StringComparison.Ordinal);

        return new StoredPaymentRequest(
            request.PaymentToken,
            request.Amount,
            request.CurrencyCode,
            new ProcessTopUpResult
            {
                MerchantTransactionId = request.MerchantTransactionId,
                ProviderTransactionId = $"PAY-{Guid.NewGuid():N}",
                Decision = isDeclined
                    ? PaymentDecision.Declined
                    : PaymentDecision.Approved,
                ProcessedAtUtc = DateTime.UtcNow,
                DeclineReason = isDeclined
                    ? "The demo provider declined the payment token."
                    : null
            });
    }

    private static bool HasSamePaymentDetails(
        StoredPaymentRequest storedRequest,
        ProcessTopUpRequest request)
    {
        return string.Equals(
                storedRequest.PaymentToken,
                request.PaymentToken,
                StringComparison.Ordinal) &&
            storedRequest.Amount == request.Amount &&
            string.Equals(
                storedRequest.CurrencyCode,
                request.CurrencyCode,
                StringComparison.OrdinalIgnoreCase);
    }

    private sealed record StoredPaymentRequest(
        string PaymentToken,
        decimal Amount,
        string CurrencyCode,
        ProcessTopUpResult Response);
}
