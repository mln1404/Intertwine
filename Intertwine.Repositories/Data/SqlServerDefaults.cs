namespace Intertwine.Repositories.Data;

/// <summary>
/// SQL Server expressions used as database column defaults.
/// </summary>
public static class SqlServerDefaults
{
    public const string UtcDateTime = "GETUTCDATE()";
}
