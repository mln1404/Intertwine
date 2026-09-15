using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Intertwine.SOAP.Authentication;

/// <summary>
/// Authenticates SOAP clients using an API key supplied in an HTTP header.
/// </summary>
public sealed class ApiKeyAuthenticationHandler
    : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "SoapApiKey";
    public const string HeaderName = "X-API-Key";

    private readonly SoapAuthenticationOptions _authenticationOptions;

    public ApiKeyAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IOptions<SoapAuthenticationOptions> authenticationOptions)
        : base(options, logger, encoder)
    {
        _authenticationOptions = authenticationOptions.Value;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(HeaderName, out var suppliedValues))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var suppliedKey = suppliedValues.ToString();
        if (!KeysMatch(suppliedKey, _authenticationOptions.ApiKey))
        {
            return Task.FromResult(
                AuthenticateResult.Fail("The supplied SOAP API key is invalid."));
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "intertwine-api"),
            new Claim(ClaimTypes.Name, "Intertwine API")
        };
        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    protected override Task HandleChallengeAsync(
        AuthenticationProperties properties)
    {
        Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    }

    private static bool KeysMatch(string suppliedKey, string expectedKey)
    {
        var suppliedBytes = Encoding.UTF8.GetBytes(suppliedKey);
        var expectedBytes = Encoding.UTF8.GetBytes(expectedKey);

        return suppliedBytes.Length == expectedBytes.Length &&
            CryptographicOperations.FixedTimeEquals(
                suppliedBytes,
                expectedBytes);
    }
}
