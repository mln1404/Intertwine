using Intertwine.Services.DTOs.Questions;
using Intertwine.Services.DTOs.UserAnswers;
using Intertwine.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Intertwine.API.Controllers;

[ApiController]
[Route("api/questions")]
[Authorize]
public class QuestionsController : ControllerBase
{
    private readonly IQuestionService _questionService;

    public QuestionsController(
        IQuestionService questionService)
    {
        _questionService = questionService;
    }

    [HttpGet]
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
    public async Task<IActionResult> AnswerQuestion(
        int questionId,
        [FromBody] SubmitAnswerRequest request,
        CancellationToken cancellationToken)
    {
        var identityUserId =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(identityUserId))
            return Unauthorized();

        try
        {
            await _questionService.SubmitAnswerAsync(
                identityUserId,
                questionId,
                request,
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
