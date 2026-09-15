namespace Intertwine.Worker;

/// <summary>
/// Controls the date window and execution intervals of the Daily Question worker.
/// </summary>
public sealed class DailyQuestionWorkerOptions
{
    public const string SectionName = "DailyQuestionWorker";

    public int PastDaysToCover { get; set; } = 1;
    public int FutureDaysToCover { get; set; } = 7;
    public double SuccessIntervalHours { get; set; } = 24;
    public double FailureRetryMinutes { get; set; } = 5;
}
