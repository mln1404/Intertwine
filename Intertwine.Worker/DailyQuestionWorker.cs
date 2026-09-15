using Intertwine.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace Intertwine.Worker;

/// <summary>
/// Periodically ensures Daily Question assignments exist for every supported
/// local calendar date around the current UTC date.
/// </summary>
public sealed class DailyQuestionWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TimeProvider _timeProvider;
    private readonly DailyQuestionWorkerOptions _options;
    private readonly ILogger<DailyQuestionWorker> _logger;

    public DailyQuestionWorker(
        IServiceScopeFactory scopeFactory,
        TimeProvider timeProvider,
        IOptions<DailyQuestionWorkerOptions> options,
        ILogger<DailyQuestionWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _timeProvider = timeProvider;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        _logger.LogInformation("Daily Question worker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            var succeeded = await TryEnsureAssignmentsAsync(stoppingToken);
            var delay = succeeded
                ? TimeSpan.FromHours(_options.SuccessIntervalHours)
                : TimeSpan.FromMinutes(_options.FailureRetryMinutes);

            try
            {
                await Task.Delay(delay, _timeProvider, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }

        _logger.LogInformation("Daily Question worker stopped.");
    }

    private async Task<bool> TryEnsureAssignmentsAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var service = scope.ServiceProvider
                .GetRequiredService<IDailyQuestionService>();

            var utcToday = DateOnly.FromDateTime(
                _timeProvider.GetUtcNow().UtcDateTime);
            var startDate = utcToday.AddDays(-_options.PastDaysToCover);
            var daysAhead =
                _options.PastDaysToCover + _options.FutureDaysToCover;

            var result = await service.EnsureDailyQuestionsAsync(
                startDate,
                daysAhead,
                cancellationToken);

            _logger.LogInformation(
                "Daily Question assignments ensured from {StartDate} through {EndDate}: {Created} created and {Existing} already existed.",
                result.StartDate,
                result.EndDate,
                result.Created,
                result.AlreadyExisted);

            return true;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return false;
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Daily Question assignment failed. The worker will retry in {RetryMinutes} minutes.",
                _options.FailureRetryMinutes);

            return false;
        }
    }
}
