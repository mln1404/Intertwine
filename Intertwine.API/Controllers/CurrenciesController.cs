using Intertwine.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Intertwine.API.Controllers;

[ApiController]
[Route("api/currencies")]
[Authorize]
/// <summary>
/// Exposes currencies available for Spark packages.
/// </summary>
public class CurrenciesController : ControllerBase
{
    private readonly ICurrencyService _currencyService;

    public CurrenciesController(ICurrencyService currencyService)
    {
        _currencyService = currencyService;
    }

    [HttpGet]
    /// <summary>Gets active currencies.</summary>
    public async Task<IActionResult> GetCurrencies(
        CancellationToken cancellationToken)
    {
        return Ok(await _currencyService.GetActiveAsync(cancellationToken));
    }
}
