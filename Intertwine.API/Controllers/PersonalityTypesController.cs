using Intertwine.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Intertwine.API.Controllers;

[ApiController]
[Route("api/personality-types")]
[Authorize]
/// <summary>
/// Exposes personality types available for profile selection.
/// </summary>
public class PersonalityTypesController : ControllerBase
{
    private readonly IPersonalityTypeService _personalityTypeService;

    public PersonalityTypesController(
        IPersonalityTypeService personalityTypeService)
    {
        _personalityTypeService = personalityTypeService;
    }

    [HttpGet]
    /// <summary>Gets active personality types ordered by code.</summary>
    public async Task<IActionResult> GetPersonalityTypes(
        CancellationToken cancellationToken)
    {
        return Ok(await _personalityTypeService.GetActiveAsync(cancellationToken));
    }
}
