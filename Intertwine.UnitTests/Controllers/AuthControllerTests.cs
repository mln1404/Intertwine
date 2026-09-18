using Intertwine.API.Controllers;
using Intertwine.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Intertwine.UnitTests.Controllers;

public class AuthControllerTests
{
    [Fact]
    public async Task Logout_WithCookie_RevokesTokenAndExpiresCookie()
    {
        var authService = new Mock<IAuthService>(MockBehavior.Strict);
        authService.Setup(x => x.LogoutAsync("raw-token"))
            .Returns(Task.CompletedTask);
        var context = new DefaultHttpContext();
        context.Request.Headers.Cookie = "refreshToken=raw-token";
        var controller = CreateController(authService, context);

        var response = await controller.Logout();

        Assert.IsType<NoContentResult>(response);
        authService.Verify(x => x.LogoutAsync("raw-token"), Times.Once);
        var cookie = context.Response.Headers.SetCookie.ToString();
        Assert.Contains("refreshToken=", cookie);
        Assert.Contains("expires=", cookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("path=/", cookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("secure", cookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("httponly", cookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("samesite=strict", cookie, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Logout_WithoutCookie_StillExpiresCookieAndReturnsNoContent()
    {
        var authService = new Mock<IAuthService>(MockBehavior.Strict);
        authService.Setup(x => x.LogoutAsync(null))
            .Returns(Task.CompletedTask);
        var context = new DefaultHttpContext();
        var controller = CreateController(authService, context);

        var response = await controller.Logout();

        Assert.IsType<NoContentResult>(response);
        authService.Verify(x => x.LogoutAsync(null), Times.Once);
        Assert.Contains("refreshToken=", context.Response.Headers.SetCookie.ToString());
    }

    [Fact]
    public async Task Logout_WhenRevocationFails_ExpiresCookieAndPropagatesFailure()
    {
        var authService = new Mock<IAuthService>(MockBehavior.Strict);
        authService.Setup(x => x.LogoutAsync("raw-token"))
            .ThrowsAsync(new InvalidOperationException("Database unavailable"));
        var context = new DefaultHttpContext();
        context.Request.Headers.Cookie = "refreshToken=raw-token";
        var controller = CreateController(authService, context);

        await Assert.ThrowsAsync<InvalidOperationException>(() => controller.Logout());

        Assert.Contains("refreshToken=", context.Response.Headers.SetCookie.ToString());
    }

    private static AuthController CreateController(
        Mock<IAuthService> authService,
        DefaultHttpContext context) =>
        new(authService.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = context }
        };
}
