namespace Intertwine.Services.DTOs.Discovery;

public class DiscoveryUsersDto
{
    public DiscoveryAccessState AccessState { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public bool HasMore { get; set; }
    public IReadOnlyList<DiscoveryUserDto> Users { get; set; } = [];
}

public class DiscoveryUserDto
{
    public int UserProfileId { get; set; }
    public string AvatarName { get; set; } = string.Empty;
    public string? PersonalityTypeCode { get; set; }
}
