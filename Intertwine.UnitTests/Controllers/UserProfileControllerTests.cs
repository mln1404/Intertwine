using System.Security.Claims;
using Intertwine.API.Controllers;
using Intertwine.Services.DTOs.UserProfiles;
using Intertwine.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Intertwine.UnitTests.Controllers;

public class UserProfileControllerTests
{
    [Fact]
    public void Controller_RequiresAuthentication()
    {
        Assert.Single(typeof(UserProfileController)
            .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true));
    }

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
    public async Task GetPublicProfile_WhenProfileExists_ReturnsPublicProfile()
    {
        var profile = new PublicUserProfileDto
        {
            UserProfileId = 12,
            AvatarName = "Public Avatar",
            PersonalityTypeCode = "ENFP",
            AnsweredQuestions =
            [
                new PublicProfileAnswerDto
                {
                    QuestionId = 7,
                    AnswerId = 20,
                    AnswerText = "Kindness"
                }
            ]
        };
        var service = new Mock<IUserProfileService>();
        service.Setup(x => x.GetPublicProfileAsync(
                12,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);
        var controller = CreateController(service);

        var response = await controller.GetPublicProfile(12, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(response);
        var returned = Assert.IsType<PublicUserProfileDto>(ok.Value);
        Assert.Equal("Public Avatar", returned.AvatarName);
        Assert.Equal("ENFP", returned.PersonalityTypeCode);
        Assert.Single(returned.AnsweredQuestions);
    }

    [Fact]
    public async Task GetPublicProfile_WhenProfileIsMissing_ReturnsNotFound()
    {
        var service = new Mock<IUserProfileService>();
        service.Setup(x => x.GetPublicProfileAsync(
                999,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((PublicUserProfileDto?)null);
        var controller = CreateController(service);

        var response = await controller.GetPublicProfile(999, CancellationToken.None);

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
