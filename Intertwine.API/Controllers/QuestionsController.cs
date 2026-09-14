using Intertwine.Services.DTOs.Questions;
using Intertwine.Services.DTOs.UserAnswers;
using Intertwine.Services.Interfaces;
using Intertwine.Services.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Intertwine.API.Controllers;

[ApiController]
[Route("api/questions")]
[Authorize]
/// <summary>
/// Exposes question retrieval and answer-submission endpoints.
/// </summary>
public class QuestionsController : ControllerBase
{
    private readonly IQuestionService _questionService;
    private readonly IUserAnswerService _userAnswerService;

    public QuestionsController(
        IQuestionService questionService,
        IUserAnswerService userAnswerService)
    {
        _questionService = questionService;
        _userAnswerService = userAnswerService;
    }

    [HttpGet]
    /// <summary>
    /// Retrieves active questions, optionally filtered by category.
    /// </summary>
    public async Task<IActionResult> GetQuestions(
        [FromQuery] int? categoryId,
        CancellationToken cancellationToken)
    {
        var questions =
            await _questionService.GetQuestionsAsync(
                categoryId,
                cancellationToken);

        return Ok(questions);
    }

    [HttpGet("{questionId:int}")]
    /// <summary>
    /// Retrieves one active question and its available answers.
    /// </summary>
    public async Task<IActionResult> GetQuestion(
        int questionId,
        CancellationToken cancellationToken)
    {
        var question =
            await _questionService.GetQuestionByIdAsync(
                questionId,
                cancellationToken);

        if (question == null)
            return NotFound();

        return Ok(question);
    }

    [HttpPost("{questionId:int}/answer")]
    /// <summary>
    /// Submits the caller's answer using the local date supplied by the client.
    /// </summary>
    public async Task<IActionResult> AnswerQuestion(
        int questionId,
        [FromBody] SubmitAnswerRequest request,
        [FromQuery] DateOnly localDate,
        CancellationToken cancellationToken)
    {
        var identityUserId =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(identityUserId))
            return Unauthorized();

        try
        {
            await _userAnswerService.SubmitAnswerAsync(
                identityUserId,
                questionId,
                request,
                localDate,
                cancellationToken);

            return Ok(new
            {
                message = "Answer submitted successfully."
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }
}
