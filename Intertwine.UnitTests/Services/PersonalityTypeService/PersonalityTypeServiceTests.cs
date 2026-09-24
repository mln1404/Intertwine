using Intertwine.Domain.Entities;
using Intertwine.Services.Interfaces.Repositories;
using Intertwine.Services.Services;
using Moq;

namespace Intertwine.UnitTests.Services.PersonalityTypeService;

public class PersonalityTypeServiceTests
{
    [Fact]
    public async Task GetActiveAsync_MapsRepositoryPersonalityTypes()
    {
        var repository = new Mock<IPersonalityTypeRepository>();
        repository.Setup(x => x.GetActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([
                new PersonalityType
                {
                    PersonalityTypeId = 8,
                    Code = "ENFP",
                    Name = "Campaigner",
                    IsActive = true
                }
            ]);
        var service = new Intertwine.Services.Services.PersonalityTypeService(
            repository.Object);

        var result = await service.GetActiveAsync();

        var personalityType = Assert.Single(result);
        Assert.Equal(8, personalityType.PersonalityTypeId);
        Assert.Equal("ENFP", personalityType.Code);
        Assert.Equal("Campaigner", personalityType.Name);
    }
}
