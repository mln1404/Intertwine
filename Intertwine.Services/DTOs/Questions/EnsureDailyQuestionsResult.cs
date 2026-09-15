namespace Intertwine.Services.DTOs.Questions;

public sealed record EnsureDailyQuestionsResult(
    DateOnly StartDate,
    DateOnly EndDate,
    int Created,
    int AlreadyExisted);
