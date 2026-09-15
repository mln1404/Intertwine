using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Intertwine.Services.DTOs.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Service = Intertwine.Services.Services.TokenService;

namespace Intertwine.UnitTests.Services.TokenService;

public class TokenServiceTests
{
    [Fact]
    public void GenerateToken_CreatesValidSignedTokenWithExpectedClaims()
    {
        var settings = new JwtSettings
        {
            Key = "unit-test-signing-key-with-at-least-32-bytes",
            Issuer = "Intertwine.UnitTests",
            Audience = "Intertwine.Client",
            ExpiryMinutes = 30
        };
        var service = new Service(Options.Create(settings));
        var beforeGeneration = DateTime.UtcNow;

        var encodedToken = service.GenerateToken(
            "identity-1",
            "person@example.com");

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = settings.Issuer,
            ValidateAudience = true,
            ValidAudience = settings.Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(settings.Key)),
            ClockSkew = TimeSpan.Zero,
            NameClaimType = ClaimTypes.Name
        };
        var principal = new JwtSecurityTokenHandler().ValidateToken(
            encodedToken,
            validationParameters,
            out var validatedToken);

        Assert.IsType<JwtSecurityToken>(validatedToken);
        Assert.Equal("identity-1", principal.FindFirstValue(ClaimTypes.NameIdentifier));
        Assert.Equal("person@example.com", principal.FindFirstValue(ClaimTypes.Name));
        Assert.InRange(
            validatedToken.ValidTo,
            beforeGeneration.AddMinutes(30).AddSeconds(-2),
            beforeGeneration.AddMinutes(30).AddSeconds(2));
    }
}
