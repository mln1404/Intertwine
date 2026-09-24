using System.Security.Claims;
using Intertwine.API.Controllers;
using Intertwine.Services.DTOs.UserProfiles;
using Intertwine.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Intertwine.UnitTests.Controllers;

public class UserProfileControllerTests
{
    [Fact]
    public async Task PreviewMe_WhenAuthenticated_ReturnsPublicProfile()
    {
        var profile = new PublicUserProfileDto
        {
            UserProfileId = 7,
            AvatarName = "Merrick Lance",
            PersonalityTypeCode = "ENFP"
        };
        var service = new Mock<IUserProfileService>();
        service.Setup(x => x.GetCurrentUserPublicProfileAsync(
                "identity-user",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);
        var controller = CreateController(service);

        var response = await controller.PreviewMe(CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(response);
        Assert.Same(profile, ok.Value);
    }

    [Fact]
    public async Task PreviewMe_WhenProfileIsMissing_ReturnsNotFound()
    {
        var service = new Mock<IUserProfileService>();
        service.Setup(x => x.GetCurrentUserPublicProfileAsync(
                "identity-user",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((PublicUserProfileDto?)null);
        var controller = CreateController(service);

        var response = await controller.PreviewMe(CancellationToken.None);

        Assert.IsType<NotFoundResult>(response);
    }

    [Fact]
    public async Task UpdateMe_WhenPersonalityTypeIsInvalid_ReturnsBadRequest()
    {
        var service = new Mock<IUserProfileService>();
        service.Setup(x => x.UpdateCurrentUserAsync(
                "identity-user",
                It.IsAny<UpdateUserProfileRequest>()))
            .ThrowsAsync(new ArgumentException(
                "The selected personality type does not exist or is inactive."));
        var controller = CreateController(service);

        var response = await controller.UpdateMe(
            new UpdateUserProfileRequest { PersonalityTypeId = 999 });

        var badRequest = Assert.IsType<BadRequestObjectResult>(response);
        Assert.Contains("does not exist or is inactive", badRequest.Value?.ToString());
    }

    private static UserProfileController CreateController(
        Mock<IUserProfileService> service)
    {
        var context = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(
                [new Claim(ClaimTypes.NameIdentifier, "identity-user")],
                "Test"))
        };

        return new UserProfileController(service.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = context }
        };
    }
}
