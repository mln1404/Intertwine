namespace Intertwine.Services.DTOs.Discovery;

public class DailyQuestionDiscoveryDto
{
    public int DailyQuestionId { get; set; }
    public DateOnly Date { get; set; }
    public int QuestionId { get; set; }
    public string QuestionTitle { get; set; } = string.Empty;
    public string FullQuestion { get; set; } = string.Empty;
    public int CurrentUserAnswerId { get; set; }
    public string CurrentUserAnswerText { get; set; } = string.Empty;
    public DiscoveryAccessState DateAccessState { get; set; }
    public IReadOnlyList<DiscoveryAnswerPoolDto> AnswerPools { get; set; } = [];
    public IReadOnlyList<DiscoveryDayDto> HistoricalAccess { get; set; } = [];
}

public class DiscoveryAnswerPoolDto
{
    public int AnswerId { get; set; }
    public string AnswerText { get; set; } = string.Empty;
    public DiscoveryAccessState AccessState { get; set; }
}

public class DiscoveryDayDto
{
    public int DailyQuestionId { get; set; }
    public DateOnly Date { get; set; }
    public string QuestionTitle { get; set; } = string.Empty;
    public bool HasAnswered { get; set; }
    public DiscoveryAccessState AccessState { get; set; }
}
