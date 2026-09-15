using Intertwine.Repositories.Data;
using Intertwine.Repositories.Repositories;
using Intertwine.Services.Interfaces;
using Intertwine.Services.Interfaces.Repositories;
using Intertwine.Services.Services;
using Intertwine.Worker;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

var sqlConnectionString = builder.Configuration.GetConnectionString(
    WorkerConfigurationKeys.DefaultConnection);
if (string.IsNullOrWhiteSpace(sqlConnectionString))
{
    throw new InvalidOperationException(
        $"Connection string '{WorkerConfigurationKeys.DefaultConnection}' is not configured.");
}

builder.Services.AddDbContext<IntertwineDbContext>(options =>
    options.UseSqlServer(sqlConnectionString));

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<IDailyQuestionRepository, DailyQuestionRepository>();
builder.Services.AddScoped<IDailyQuestionService, DailyQuestionService>();

builder.Services
    .AddOptions<DailyQuestionWorkerOptions>()
    .Bind(builder.Configuration.GetSection(
        DailyQuestionWorkerOptions.SectionName))
    .Validate(
        options => options.PastDaysToCover >= 0,
        "PastDaysToCover cannot be negative.")
    .Validate(
        options => options.FutureDaysToCover >= 0,
        "FutureDaysToCover cannot be negative.")
    .Validate(
        options => options.SuccessIntervalHours > 0,
        "SuccessIntervalHours must be greater than zero.")
    .Validate(
        options => options.FailureRetryMinutes > 0,
        "FailureRetryMinutes must be greater than zero.")
    .ValidateOnStart();

builder.Services.AddHostedService<DailyQuestionWorker>();

var host = builder.Build();
await host.RunAsync();
