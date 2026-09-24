using Intertwine.Services.DTOs.PersonalityTypes;
using Intertwine.Services.Interfaces;
using Intertwine.Services.Interfaces.Repositories;

namespace Intertwine.Services.Services;

/// <summary>
/// Maps active personality types to API response data.
/// </summary>
public class PersonalityTypeService : IPersonalityTypeService
{
    private readonly IPersonalityTypeRepository _personalityTypeRepository;

    public PersonalityTypeService(
        IPersonalityTypeRepository personalityTypeRepository)
    {
        _personalityTypeRepository = personalityTypeRepository;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PersonalityTypeDto>> GetActiveAsync(
        CancellationToken cancellationToken = default)
    {
        var personalityTypes = await _personalityTypeRepository
            .GetActiveAsync(cancellationToken);

        return personalityTypes.Select(x => new PersonalityTypeDto
        {
            PersonalityTypeId = x.PersonalityTypeId,
            Code = x.Code,
            Name = x.Name
        }).ToList();
    }
}
