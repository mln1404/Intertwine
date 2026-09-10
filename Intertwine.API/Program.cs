using Intertwine.API.Models;
using Intertwine.Identity;
using Intertwine.Repositories.Data;
using Intertwine.Services.Interfaces;
using Intertwine.Services.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Configure JwtSettings from configuration
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

// Register token service
builder.Services.AddScoped<ITokenService, TokenService>();

// Configure EF Core and Identity
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
}

builder.Services.AddDbContext<IntertwineDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<IntertwineDbContext>()
    .AddDefaultTokenProviders();

// Service registrations
builder.Services.AddScoped<IAuthService, AuthService>();

// Repository registrations
builder.Services.AddScoped<IUserProfileRepository, UserProfileRepository>();

// Configure authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
if (jwtSettings is null)
{
    throw new InvalidOperationException("JWT settings are not configured. Check appsettings.json.");
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

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
