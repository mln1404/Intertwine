using System.Security.Claims;
using Intertwine.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Intertwine.API.Controllers;

/// <summary>
/// Exposes the authenticated user's daily answer allowance state.
/// </summary>
[ApiController]
[Route("api/daily-activity")]
[Authorize]
public class DailyActivityController : ControllerBase
{
    private readonly IUserDailyActivityService _dailyActivityService;

    public DailyActivityController(
        IUserDailyActivityService dailyActivityService)
    {
        _dailyActivityService = dailyActivityService;
    }

    [HttpGet("me")]
    /// <summary>Gets the caller's answer allowance and Daily Question state for a local date.</summary>
    public async Task<IActionResult> GetMe(
        [FromQuery] DateOnly localDate,
        CancellationToken cancellationToken)
    {
        var identityUserId =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(identityUserId))
        {
            return Unauthorized();
        }

        var activity = await _dailyActivityService
            .GetCurrentUserActivityAsync(
                identityUserId,
                localDate,
                cancellationToken);

        return Ok(activity);
    }
}
