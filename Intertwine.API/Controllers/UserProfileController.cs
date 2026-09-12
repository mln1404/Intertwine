using Intertwine.Services.DTOs.UserProfiles;
using Intertwine.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Intertwine.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserProfileController : ControllerBase
{
    private readonly IUserProfileService _userProfileService;

    public UserProfileController(
        IUserProfileService userProfileService)
    {
        _userProfileService = userProfileService;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        var identityUserId =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(identityUserId))
            return Unauthorized();

        var profile = await _userProfileService
            .GetCurrentUserProfileAsync(identityUserId);

        if (profile is null)
            return NotFound();

        return Ok(profile);
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateMe(
        UpdateUserProfileRequest request)
    {
        var identityUserId =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(identityUserId))
            return Unauthorized();

        var profile = await _userProfileService
            .UpdateCurrentUserAsync(
                identityUserId,
                request);

        if (profile is null)
            return NotFound();

        return Ok(profile);
    }

    [HttpDelete("me")]
    public async Task<IActionResult> DeleteMe()
    {
        var identityUserId =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(identityUserId))
            return Unauthorized();

        var deleted = await _userProfileService
            .DeleteCurrentUserAsync(identityUserId);

        if (!deleted)
            return NotFound();

        return NoContent();
    }
}