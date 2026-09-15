using Intertwine.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Intertwine.API.Controllers;

[ApiController]
[Route("api/credit-packages")]
[Authorize]
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
