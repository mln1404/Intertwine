using Intertwine.Services.DTOs.UserProfiles;
using Intertwine.Services.Interfaces;
using Intertwine.Services.Services;
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

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var profiles = await _userProfileService.GetAllAsync();

        return Ok(profiles);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var profile =
            await _userProfileService.GetByIdAsync(id);

        if (profile is null)
        {
            return NotFound();
        }

        return Ok(profile);
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        var identityUserId =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(identityUserId))
        {
            return Unauthorized();
        }

        var profile =
            await _userProfileService
                .GetCurrentUserProfileAsync(identityUserId);

        if (profile is null)
        {
            return NotFound();
        }

        return Ok(profile);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        CreateUserProfileRequest request)
    {
        var profile =
            await _userProfileService.CreateAsync(request);

        if (profile is null)
        {
            return Conflict(
                "A profile already exists for this user.");
        }

        return CreatedAtAction(
            nameof(GetById),
            new { id = profile.UserProfileId },
            profile);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        UpdateUserProfileRequest request)
    {
        var profile =
            await _userProfileService.UpdateAsync(id, request);

        if (profile is null)
        {
            return NotFound();
        }

        return Ok(profile);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted =
            await _userProfileService.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}