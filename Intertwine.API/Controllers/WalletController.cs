using System.Security.Claims;
using Intertwine.Services.DTOs.Wallets;
using Intertwine.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Intertwine.API.Controllers;

[ApiController]
[Route("api/wallet")]
[Authorize]
/// <summary>
/// Exposes the authenticated user's Spark balance, demo top-up, and payment history.
/// </summary>
public class WalletController : ControllerBase
{
    private readonly IWalletService _walletService;

    public WalletController(
        IWalletService walletService)
    {
        _walletService = walletService;
    }

    [HttpGet]
    /// <summary>Gets the caller's current Spark balance.</summary>
    public async Task<IActionResult> GetWallet(
        CancellationToken cancellationToken)
    {
        var identityUserId =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(identityUserId))
        {
            return Unauthorized();
        }

        var wallet =
            await _walletService.GetWalletAsync(
                identityUserId,
                cancellationToken);

        return Ok(wallet);
    }

    [HttpPost("top-up")]
    /// <summary>Completes the current demo top-up for a selected credit package.</summary>
    public async Task<IActionResult> TopUp(
        [FromBody] TopUpRequest request,
        CancellationToken cancellationToken)
    {
        var identityUserId =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(identityUserId))
        {
            return Unauthorized();
        }

        var result =
            await _walletService.TopUpAsync(
                identityUserId,
                request,
                cancellationToken);

        return Ok(result);
    }

    [HttpGet("payments")]
    /// <summary>Gets a page of the caller's payment history.</summary>
    public async Task<IActionResult> GetPayments([FromQuery] int page = 1, CancellationToken cancellationToken = default)
    {
        var identityUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(identityUserId)) return Unauthorized();
        if (page < 1 || page > 100000) return BadRequest(new { message = "Invalid page number." });
        return Ok(await _walletService.GetPaymentsAsync(identityUserId, page, cancellationToken));
    }
}
