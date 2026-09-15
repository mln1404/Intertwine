namespace Intertwine.Services.Constants;

/// <summary>Server-owned pricing for optional question actions.</summary>
public static class SparkRules
{
    public const long ExtraAnswerCost = 10;
    public const string InsufficientBalance = "You need 10 Sparks for an extra answer. Get more Sparks and try again.";
    public const string ExtraAnswerDescription = "Extra non-daily answer or update";
}
