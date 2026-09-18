using Intertwine.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Intertwine.API.Controllers;

[ApiController]
[Route("api/credit-packages")]
[Authorize]
/// <summary>
/// Exposes active Spark packages for an authenticated user.
/// </summary>
public class CreditPackagesController : ControllerBase
{
    private readonly ICreditPackageService
        _creditPackageService;

    public CreditPackagesController(
        ICreditPackageService creditPackageService)
    {
        _creditPackageService = creditPackageService;
    }

    [HttpGet]
    /// <summary>Gets active packages priced in a requested currency.</summary>
    public async Task<IActionResult> GetCreditPackages(
        [FromQuery] string currencyCode,
        CancellationToken cancellationToken)
    {
        var packages =
            await _creditPackageService.GetActiveAsync(
                currencyCode,
                cancellationToken);

        return Ok(packages);
    }
}
