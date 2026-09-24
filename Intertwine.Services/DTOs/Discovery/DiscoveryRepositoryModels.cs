namespace Intertwine.Services.DTOs.Discovery;

public sealed record DiscoveryContextData(
    int DailyQuestionId,
    DateOnly Date,
    int QuestionId,
    string QuestionTitle,
    string FullQuestion,
    int? CurrentUserAnswerId,
    string? CurrentUserAnswerText,
    IReadOnlyList<DiscoveryAnswerData> Answers);

public sealed record DiscoveryAnswerData(int AnswerId, string AnswerText);

public sealed record DiscoveryHistoryData(
    int DailyQuestionId,
    DateOnly Date,
    string QuestionTitle,
    bool HasAnswered);

public sealed record DiscoveryUsersPageData(
    int TotalCount,
    IReadOnlyList<DiscoveryUserDto> Users);
