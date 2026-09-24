using System.Security.Claims;
using Intertwine.API.Controllers;
using Intertwine.Services.DTOs.Discovery;
using Intertwine.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Intertwine.UnitTests.Controllers;

public class DailyQuestionsControllerTests
{
    [Fact]
    public async Task GetDiscoveryUsers_WhenPoolRequiresSparks_ReturnsForbiddenState()
    {
        var service = new Mock<IDailyQuestionDiscoveryService>();
        service.Setup(x => x.GetUsersAsync(
                "identity-user", 5, 11, new DateOnly(2026, 9, 24), 1, 20,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DiscoveryUsersDto
            {
                AccessState = DiscoveryAccessState.SparksRequired,
                Page = 1,
                PageSize = 20
            });
        var controller = CreateController(service);

        var response = await controller.GetDiscoveryUsers(
            5, 11, new DateOnly(2026, 9, 24), 1, 20);

        var forbidden = Assert.IsType<ObjectResult>(response);
        Assert.Equal(StatusCodes.Status403Forbidden, forbidden.StatusCode);
        var result = Assert.IsType<DiscoveryUsersDto>(forbidden.Value);
        Assert.Equal(DiscoveryAccessState.SparksRequired, result.AccessState);
        Assert.Empty(result.Users);
    }

    [Fact]
    public async Task GetDiscovery_WhenDailyQuestionDoesNotExist_ReturnsNotFound()
    {
        var service = new Mock<IDailyQuestionDiscoveryService>();
        service.Setup(x => x.GetContextAsync(
                "identity-user", 999, new DateOnly(2026, 9, 24),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((DailyQuestionDiscoveryDto?)null);
        var controller = CreateController(service);

        var response = await controller.GetDiscovery(
            999, new DateOnly(2026, 9, 24), CancellationToken.None);

        Assert.IsType<NotFoundResult>(response);
    }

    private static DailyQuestionsController CreateController(
        Mock<IDailyQuestionDiscoveryService> service)
    {
        var context = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(
                [new Claim(ClaimTypes.NameIdentifier, "identity-user")],
                "Test"))
        };

        return new DailyQuestionsController(service.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = context }
        };
    }
}
