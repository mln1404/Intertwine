namespace Intertwine.Services.Constants;

/// <summary>
/// User-facing messages returned by authentication operations.
/// </summary>
public static class AuthMessages
{
    public const string DuplicateEmail = "An account with this email already exists.";
    public const string InvalidCredentials = "Invalid email or password.";
    public const string ErrorDelimiter = "; ";
}
