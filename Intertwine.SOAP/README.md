# Intertwine SOAP Payment Provider

This project simulates an external payment provider. It publishes a SOAP 1.1 contract through CoreWCF and does not update the Intertwine database or wallet directly.

## Run the provider

```powershell
dotnet run --project Intertwine.SOAP --launch-profile https
```

The WSDL is available at:

```text
https://localhost:7019/PaymentGateway.svc?wsdl
```

## Authentication

`ProcessTopUp` requires an API key in the HTTP header below:

```http
X-API-Key: intertwine-soap-local-key-change-me
```

The checked-in key is a local-development credential from `appsettings.Development.json`. Set `SoapAuthentication__ApiKey` through a protected configuration provider outside local development. The endpoint uses HTTPS because an API key must not travel over an unencrypted connection.

ASP.NET Core's authentication handler validates the key and creates an authenticated caller identity. The CoreWCF operation uses an authorization policy to require that identity before executing payment code. Missing or invalid credentials receive `401 Unauthorized`.

## SOAP request

```http
POST https://localhost:7019/PaymentGateway.svc
Content-Type: text/xml; charset=utf-8
SOAPAction: "https://intertwine.example.com/contracts/payments/v1/IPaymentGatewayService/ProcessTopUp"
X-API-Key: intertwine-soap-local-key-change-me

<s:Envelope xmlns:s="http://schemas.xmlsoap.org/soap/envelope/">
  <s:Body>
    <ProcessTopUp xmlns="https://intertwine.example.com/contracts/payments/v1">
      <request>
        <MerchantTransactionId>top-up-0001</MerchantTransactionId>
        <PaymentToken>tok_approved</PaymentToken>
        <Amount>9.99</Amount>
        <CurrencyCode>AUD</CurrencyCode>
      </request>
    </ProcessTopUp>
  </s:Body>
</s:Envelope>
```

Any non-empty demo token is approved except `tok_declined`, which returns a declined decision. Real card details are deliberately absent; a payment token represents details collected by a payment provider.

`MerchantTransactionId` is the caller's idempotency identifier. Repeating the same payment returns the original provider response. Reusing the identifier with different payment details returns a SOAP fault. The demo store is in memory, so its records reset when this project restarts.
