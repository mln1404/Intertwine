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
    public async Task UpdateMe_WhenPersonalityTypeIsInvalid_ReturnsBadRequest()
    {
        var service = new Mock<IUserProfileService>();
        service.Setup(x => x.UpdateCurrentUserAsync(
                "identity-user",
                It.IsAny<UpdateUserProfileRequest>()))
            .ThrowsAsync(new ArgumentException(
                "The selected personality type does not exist or is inactive."));
        var context = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(
                [new Claim(ClaimTypes.NameIdentifier, "identity-user")],
                "Test"))
        };
        var controller = new UserProfileController(service.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = context }
        };

        var response = await controller.UpdateMe(
            new UpdateUserProfileRequest { PersonalityTypeId = 999 });

        var badRequest = Assert.IsType<BadRequestObjectResult>(response);
        Assert.Contains("does not exist or is inactive", badRequest.Value?.ToString());
    }
}
