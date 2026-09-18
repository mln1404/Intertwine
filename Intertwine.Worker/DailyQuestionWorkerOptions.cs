namespace Intertwine.Worker;

/// <summary>
/// Controls the date window and execution intervals of the Daily Question worker.
/// </summary>
public sealed class DailyQuestionWorkerOptions
{
    /// <summary>Gets the configuration section bound to these options.</summary>
    public const string SectionName = "DailyQuestionWorker";

    /// <summary>Gets or sets how many earlier local dates the worker ensures.</summary>
    public int PastDaysToCover { get; set; } = 1;

    /// <summary>Gets or sets how many future local dates the worker ensures.</summary>
    public int FutureDaysToCover { get; set; } = 7;

    /// <summary>Gets or sets the delay after a successful assignment run.</summary>
    public double SuccessIntervalHours { get; set; } = 24;

    /// <summary>Gets or sets the retry delay after an assignment failure.</summary>
    public double FailureRetryMinutes { get; set; } = 5;
}
