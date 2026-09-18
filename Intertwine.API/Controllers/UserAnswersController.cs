using System.Security.Claims;
using Intertwine.Services.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Intertwine.API.Controllers;

/// <summary>
/// Exposes the authenticated user's current question answers.
/// </summary>
[ApiController]
[Route("api/user-answers")]
[Authorize]
public class UserAnswersController : ControllerBase
{
    private readonly IUserAnswerService _userAnswerService;

    public UserAnswersController(IUserAnswerService userAnswerService)
    {
        _userAnswerService = userAnswerService;
    }

    [HttpGet("me")]
    /// <summary>Gets the caller's current answer selection for every answered question.</summary>
    public async Task<IActionResult> GetMe(
        CancellationToken cancellationToken)
    {
        var identityUserId =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(identityUserId))
        {
            return Unauthorized();
        }

        var answers = await _userAnswerService.GetCurrentAnswersAsync(
            identityUserId,
            cancellationToken);

        return Ok(answers);
    }
}
