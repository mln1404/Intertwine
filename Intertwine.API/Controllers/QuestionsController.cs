using Intertwine.Services.DTOs.Questions;
using Intertwine.Services.DTOs.UserAnswers;
using Intertwine.Services.Constants;
using Intertwine.Services.Interfaces;
using Intertwine.Services.Interfaces.Repositories;
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
    private readonly IAnswerSubmissionIdempotencyStore _idempotencyStore;

    public QuestionsController(
        IQuestionService questionService,
        IUserAnswerService userAnswerService,
        IAnswerSubmissionIdempotencyStore idempotencyStore)
    {
        _questionService = questionService;
        _userAnswerService = userAnswerService;
        _idempotencyStore = idempotencyStore;
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
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        CancellationToken cancellationToken)
    {
        var identityUserId =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(identityUserId))
            return Unauthorized();

        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            return BadRequest(new
            {
                message = IdempotencyMessages.KeyRequired
            });
        }

        var requestFingerprint =
            $"{questionId}:{request.AnswerId}:{localDate:O}";
        var idempotencyStatus = await _idempotencyStore.TryAcquireAsync(
            identityUserId,
            idempotencyKey,
            requestFingerprint,
            cancellationToken);

        if (idempotencyStatus == AnswerSubmissionIdempotencyStatus.Completed)
        {
            return Ok(new
            {
                message = "Answer submitted successfully."
            });
        }

        if (idempotencyStatus == AnswerSubmissionIdempotencyStatus.InProgress)
        {
            return Conflict(new
            {
                message = IdempotencyMessages.RequestInProgress
            });
        }

        if (idempotencyStatus ==
            AnswerSubmissionIdempotencyStatus.KeyUsedForDifferentRequest)
        {
            return Conflict(new
            {
                message = IdempotencyMessages.KeyUsedForDifferentRequest
            });
        }

        try
        {
            await _userAnswerService.SubmitAnswerAsync(
                identityUserId,
                questionId,
                request,
                localDate,
                cancellationToken);

            await _idempotencyStore.CompleteAsync(
                identityUserId,
                idempotencyKey,
                requestFingerprint,
                cancellationToken);

            return Ok(new
            {
                message = "Answer submitted successfully."
            });
        }
        catch (ArgumentException ex)
        {
            await _idempotencyStore.ReleaseAsync(
                identityUserId,
                idempotencyKey,
                requestFingerprint,
                cancellationToken);

            return BadRequest(new
            {
                message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            await _idempotencyStore.ReleaseAsync(
                identityUserId,
                idempotencyKey,
                requestFingerprint,
                cancellationToken);

            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }
}
