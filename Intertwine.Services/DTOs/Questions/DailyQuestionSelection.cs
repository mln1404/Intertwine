namespace Intertwine.Services.DTOs.Questions;

/// <summary>Active question candidates and usage across all scheduled dates.</summary>
public sealed record DailyQuestionSelection(
    IReadOnlyList<DailyQuestionCandidate> Candidates,
    IReadOnlyDictionary<int, int> CategoryUsage);

public sealed record DailyQuestionCandidate(
    int QuestionId,
    IReadOnlyList<int> CategoryIds,
    DateOnly? LastSelectedDate);
