using Intertwine.Services.DTOs.Answers;
using Intertwine.Services.DTOs.Categories;

namespace Intertwine.Services.DTOs.Questions;

public class QuestionDetailDto
{
    public int QuestionId { get; set; }
    public string QuestionTitle { get; set; } = string.Empty;
    public string FullQuestion { get; set; } = string.Empty;

    public List<CategoryDto> Categories { get; set; } = [];
    public List<AnswerDto> Answers { get; set; } = [];
}