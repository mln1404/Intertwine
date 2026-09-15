using Intertwine.API.Constants;
using Intertwine.API.Middleware;
using Intertwine.Identity;
using Intertwine.Repositories.Data;
using Intertwine.Repositories.Caching;
using Intertwine.Repositories.Repositories;
using Intertwine.Services.DTOs.Authentication;
using Intertwine.Services.Interfaces;
using Intertwine.Services.Interfaces.Repositories;
using Intertwine.Services.Interfaces.Services;
using Intertwine.Services.Services;
using Intertwine.Services.Services.Questions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Register token service
builder.Services.AddScoped<ITokenService, TokenService>();

// Configure EF Core and Identity
var connectionString = builder.Configuration.GetConnectionString(
    ApplicationSettings.DefaultConnection);
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException(
        $"Connection string '{ApplicationSettings.DefaultConnection}' is not configured.");
}

builder.Services.AddDbContext<IntertwineDbContext>(options =>
    options.UseSqlServer(connectionString));

var redisConnectionString = builder.Configuration.GetConnectionString(
    ApplicationSettings.RedisConnection);
if (string.IsNullOrWhiteSpace(redisConnectionString))
{
    throw new InvalidOperationException(
        $"Connection string '{ApplicationSettings.RedisConnection}' is not configured.");
}

builder.Services.AddSingleton<IConnectionMultiplexer>(
    _ => ConnectionMultiplexer.Connect(redisConnectionString));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<IntertwineDbContext>()
    .AddDefaultTokenProviders();

// Service registrations
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IQuestionService, QuestionService>();
builder.Services.AddScoped<IUserAnswerService, UserAnswerService>();
builder.Services.AddScoped<IUserProfileService, UserProfileService>();

// Repository registrations
builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();
builder.Services.AddScoped<IUserAnswerRepository, UserAnswerRepository>();
builder.Services.AddScoped<IUserDailyActivityRepository, UserDailyActivityRepository>();
builder.Services.AddScoped<IUserProfileRepository, UserProfileRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IAnswerSubmissionIdempotencyStore,
    RedisAnswerSubmissionIdempotencyStore>();

// Configure JwtSettings from configuration
builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection(ApplicationSettings.JwtSettings));

// Configure authentication
var jwtSettings = builder.Configuration
    .GetSection(ApplicationSettings.JwtSettings)
    .Get<JwtSettings>();
if (jwtSettings is null)
{
    throw new InvalidOperationException(
        "JWT settings are not configured. Check appsettings.json.");
}
if (string.IsNullOrWhiteSpace(jwtSettings?.Key))
{
    throw new InvalidOperationException(
        "JwtSettings.Key is empty!");
}

var keyBytes = Encoding.UTF8.GetBytes(jwtSettings.Key);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(keyBytes)
        };
    });

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter(ApplicationSettings.LoginRateLimitPolicy, limiterOptions =>
    {
        limiterOptions.PermitLimit = 5;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueLimit = 0;
    });

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseRateLimiter();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
