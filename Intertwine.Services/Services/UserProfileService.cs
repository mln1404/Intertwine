using Intertwine.Domain.Entities;
using Intertwine.Services.Constants;
using Intertwine.Services.DTOs.UserProfiles;
using Intertwine.Services.Interfaces;
using Intertwine.Services.Interfaces.Repositories;

namespace Intertwine.Services.Services;

/// <summary>
/// Manages creation, retrieval, editing, and deactivation of application profiles.
/// </summary>
public class UserProfileService : IUserProfileService
{
    private readonly IUserProfileRepository _userProfileRepository;
    private readonly IPersonalityTypeRepository _personalityTypeRepository;

    public UserProfileService(
        IUserProfileRepository userProfileRepository,
        IPersonalityTypeRepository personalityTypeRepository)
    {
        _userProfileRepository = userProfileRepository;
        _personalityTypeRepository = personalityTypeRepository;
    }

    /// <inheritdoc />
    public async Task<UserProfileDto?> CreateCurrentUserAsync(
        string identityUserId,
        CreateUserProfileRequest request)
    {
        var existingProfile = await _userProfileRepository
            .GetByIdentityUserIdIncludingInactiveAsync(identityUserId);

        if (existingProfile is not null)
            return null;

        var personalityType = await GetPersonalityTypeAsync(
            request.PersonalityTypeId);

        var userProfile = new UserProfile
        {
            IdentityUserId = identityUserId,
            AvatarName = request.AvatarName,
            FirstName = request.FirstName,
            LastName = request.LastName,
            MiddleName = request.MiddleName,
            PersonalityTypeId = personalityType?.PersonalityTypeId,
            DateCreated = DateTime.UtcNow,
            CreatedBy = identityUserId,
            UserWallet = new UserWallet
            {
                CreditBalance = 0,
                DateCreated = DateTime.UtcNow,
                CreatedBy = identityUserId
            }
        };

        await _userProfileRepository.AddAsync(userProfile);

        return MapToDto(userProfile, personalityType?.Code);
    }

    /// <inheritdoc />
    public async Task<UserProfileDto?> GetCurrentUserProfileAsync(
        string identityUserId)
    {
        var userProfile = await _userProfileRepository
            .GetByIdentityUserIdAsync(identityUserId);

        if (userProfile is null)
            return null;

        return MapToDto(userProfile);
    }

    /// <inheritdoc />
    public async Task<PublicUserProfileDto?> GetCurrentUserPublicProfileAsync(
        string identityUserId,
        CancellationToken cancellationToken = default)
    {
        var userProfile = await _userProfileRepository
            .GetPublicProfileByIdentityUserIdAsync(
                identityUserId,
                cancellationToken);

        return userProfile is null
            ? null
            : MapToPublicDto(userProfile);
    }

    /// <inheritdoc />
    public async Task<UserProfileDto?> UpdateCurrentUserAsync(
        string identityUserId,
        UpdateUserProfileRequest request)
    {
        var userProfile = await _userProfileRepository
            .GetByIdentityUserIdAsync(identityUserId);

        if (userProfile is null)
            return null;

        var personalityType = await GetPersonalityTypeAsync(
            request.PersonalityTypeId);

        userProfile.AvatarName = request.AvatarName;
        userProfile.FirstName = request.FirstName;
        userProfile.LastName = request.LastName;
        userProfile.MiddleName = request.MiddleName;
        userProfile.PersonalityTypeId = personalityType?.PersonalityTypeId;
        userProfile.PersonalityType = null;

        await _userProfileRepository.UpdateAsync(userProfile);

        return MapToDto(userProfile, personalityType?.Code);
    }

    /// <inheritdoc />
    public async Task<bool> DeactivateCurrentUserAsync(
        string identityUserId)
    {
        var userProfile = await _userProfileRepository
            .GetByIdentityUserIdAsync(identityUserId);

        if (userProfile is null)
            return false;

        await _userProfileRepository.SetIsActiveAsync(
            userProfile,
            false,
            identityUserId);

        return true;
    }

    private async Task<PersonalityType?> GetPersonalityTypeAsync(
        int? personalityTypeId)
    {
        if (personalityTypeId is null)
            return null;

        var personalityType = await _personalityTypeRepository
            .GetActiveByIdAsync(personalityTypeId.Value);

        if (personalityType is null)
            throw new ArgumentException(UserProfileMessages.InvalidPersonalityType);

        return personalityType;
    }

    private static UserProfileDto MapToDto(
        UserProfile userProfile,
        string? personalityTypeCode = null)
    {
        return new UserProfileDto
        {
            UserProfileId = userProfile.UserProfileId,
            AvatarName = userProfile.AvatarName,
            FirstName = userProfile.FirstName,
            LastName = userProfile.LastName,
            MiddleName = userProfile.MiddleName,
            IdentityUserId = userProfile.IdentityUserId,
            CreditBalance = userProfile.UserWallet?.CreditBalance ?? 0,
            PersonalityTypeId = userProfile.PersonalityTypeId,
            PersonalityTypeCode = personalityTypeCode ?? userProfile.PersonalityType?.Code
        };
    }

    private static PublicUserProfileDto MapToPublicDto(
        UserProfile userProfile)
    {
        return new PublicUserProfileDto
        {
            UserProfileId = userProfile.UserProfileId,
            AvatarName = userProfile.AvatarName,
            PersonalityTypeCode = userProfile.PersonalityType?.Code,
            AnsweredQuestions = userProfile.UserAnswers
                .OrderBy(x => x.Answer.Question.QuestionId)
                .Select(x => new PublicProfileAnswerDto
                {
                    QuestionId = x.Answer.Question.QuestionId,
                    QuestionTitle = x.Answer.Question.QuestionTitle,
                    FullQuestion = x.Answer.Question.FullQuestion,
                    AnswerId = x.AnswerId,
                    AnswerText = x.Answer.AnswerText,
                    Categories = x.Answer.Question.QuestionCategories
                        .OrderBy(qc => qc.Category.CategoryName)
                        .Select(qc => new Intertwine.Services.DTOs.Categories.CategoryDto
                        {
                            CategoryId = qc.CategoryId,
                            CategoryName = qc.Category.CategoryName,
                            Color = qc.Category.Color
                        })
                        .ToList()
                })
                .ToList()
        };
    }
}
