using Intertwine.Services.DTOs.UserProfiles;
using Intertwine.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Intertwine.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
/// <summary>
/// Exposes endpoints for the authenticated user's profile.
/// </summary>
public class UserProfileController : ControllerBase
{
    private readonly IUserProfileService _userProfileService;

    public UserProfileController(
        IUserProfileService userProfileService)
    {
        _userProfileService = userProfileService;
    }

    [HttpPost("me")]
    /// <summary>
    /// Creates a profile for an authenticated account that does not have one yet.
    /// </summary>
    public async Task<IActionResult> CreateMe(
        CreateUserProfileRequest request)
    {
        var identityUserId =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(identityUserId))
            return Unauthorized();

        var profile = await _userProfileService
            .CreateCurrentUserAsync(identityUserId, request);

        if (profile is null)
        {
            return Conflict(new
            {
                message = "A profile already exists for this account."
            });
        }

        return CreatedAtAction(nameof(GetMe), profile);
    }

    [HttpGet("me")]
    /// <summary>
    /// Retrieves the authenticated user's active profile.
    /// </summary>
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
    /// <summary>
    /// Updates the authenticated user's active profile.
    /// </summary>
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

    [HttpPost("me/deactivate")]
    /// <summary>
    /// Deactivates the authenticated user's profile without deleting its data.
    /// </summary>
    public async Task<IActionResult> DeactivateMe()
    {
        var identityUserId =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(identityUserId))
            return Unauthorized();

        var deactivated = await _userProfileService
            .DeactivateCurrentUserAsync(identityUserId);

        if (!deactivated)
            return NotFound();

        return NoContent();
    }
}
