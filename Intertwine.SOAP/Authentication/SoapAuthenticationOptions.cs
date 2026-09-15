namespace Intertwine.SOAP.Authentication;

/// <summary>
/// Contains credentials accepted from callers of the SOAP payment service.
/// </summary>
public sealed class SoapAuthenticationOptions
{
    public const string SectionName = "SoapAuthentication";

    public string ApiKey { get; set; } = string.Empty;
}
