using System.Security.Claims;
using Intertwine.Services.DTOs.Discovery;
using Intertwine.Services.Constants;
using Intertwine.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Intertwine.API.Controllers;

[ApiController]
[Authorize]
[Route("api/daily-questions")]
public class DailyQuestionsController(
    IDailyQuestionDiscoveryService discoveryService) : ControllerBase
{
    [HttpGet("{dailyQuestionId:int}/discovery")]
    public async Task<IActionResult> GetDiscovery(
        int dailyQuestionId,
        [FromQuery] DateOnly localDate,
        CancellationToken cancellationToken)
    {
        var identityUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(identityUserId))
            return Unauthorized();

        try
        {
            var result = await discoveryService.GetContextAsync(
                identityUserId,
                dailyQuestionId,
                localDate,
                cancellationToken);
            return result is null ? NotFound() : Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{dailyQuestionId:int}/discovery/users")]
    public async Task<IActionResult> GetDiscoveryUsers(
        int dailyQuestionId,
        [FromQuery] int answerId,
        [FromQuery] DateOnly localDate,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = DailyQuestionDiscoveryRules.DefaultPageSize,
        CancellationToken cancellationToken = default)
    {
        var identityUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(identityUserId))
            return Unauthorized();

        try
        {
            var result = await discoveryService.GetUsersAsync(
                identityUserId,
                dailyQuestionId,
                answerId,
                localDate,
                page,
                pageSize,
                cancellationToken);
            if (result is null)
                return NotFound();

            return result.AccessState == DiscoveryAccessState.Included
                ? Ok(result)
                : StatusCode(StatusCodes.Status403Forbidden, result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
