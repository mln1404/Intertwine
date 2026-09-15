using CoreWCF;
using CoreWCF.Configuration;
using CoreWCF.Description;
using Intertwine.SOAP.Authentication;
using Intertwine.SOAP.Contracts;
using Intertwine.SOAP.Services;
using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    // CoreWCF serializes SOAP messages synchronously in parts of its pipeline.
    options.AllowSynchronousIO = true;
});

builder.Services
    .AddOptions<SoapAuthenticationOptions>()
    .Bind(builder.Configuration.GetSection(
        SoapAuthenticationOptions.SectionName))
    .Validate(
        options => !string.IsNullOrWhiteSpace(options.ApiKey),
        "A SOAP API key must be configured.")
    .ValidateOnStart();

builder.Services
    .AddAuthentication(ApiKeyAuthenticationHandler.SchemeName)
    .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(
        ApiKeyAuthenticationHandler.SchemeName,
        _ => { });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(
        SoapAuthorizationPolicies.PaymentProvider,
        policy => policy.RequireAuthenticatedUser());
});

builder.Services
    .AddServiceModelServices()
    .AddServiceModelMetadata();
builder.Services.AddSingleton<IServiceBehavior,
    UseRequestHeadersForMetadataAddressBehavior>();
builder.Services.AddSingleton<IPaymentRequestStore, InMemoryPaymentRequestStore>();
builder.Services.AddTransient<PaymentGatewayService>();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.UseServiceModel(serviceBuilder =>
{
    var binding = new BasicHttpBinding(
        CoreWCF.Channels.BasicHttpSecurityMode.Transport);
    binding.Security.Transport.ClientCredentialType =
        HttpClientCredentialType.InheritedFromHost;

    serviceBuilder
        .AddService<PaymentGatewayService>(options =>
        {
            options.DebugBehavior.IncludeExceptionDetailInFaults =
                app.Environment.IsDevelopment();
        })
        .AddServiceEndpoint<PaymentGatewayService, IPaymentGatewayService>(
            binding,
            "/PaymentGateway.svc");
});

var metadata = app.Services.GetRequiredService<ServiceMetadataBehavior>();
metadata.HttpsGetEnabled = true;

app.MapGet("/", () => Results.Ok(new
{
    service = "Intertwine demo SOAP payment provider",
    wsdl = "/PaymentGateway.svc?wsdl"
}));

app.Run();
